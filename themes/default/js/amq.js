/**
 *
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 * http:
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */










var org = org || {};
org.activemq = org.activemq || {};

org.activemq.Amq = function() {
	var connectStatusHandler;

	
	var adapter = org.activemq.AmqAdapter;

	if (typeof adapter == 'undefined') {
		throw 'An org.activemq.AmqAdapter must be declared before the amq.js script file.'
	}

	
	var uri;

	
	
	var timeout;

	
	
	
	var sessionInitialized = false;

	
	var sessionInitializedCallback;	

	
	
	var pollDelay;

	
	var logging = false;

	
	
	var pollErrorDelay = 5000;

	
	
	
	var messageHandlers = {};

	
	var batchInProgress = false;

	
	
	
	
	var messageQueue = [];

  
  
  
  var clientId = null;
  
	/**
	 * Iterate over the returned XML and for each message in the response, 
	 * invoke the handler with the matching id.
	 */
	var messageHandler = function(data) {
		var response = data.getElementsByTagName("ajax-response");
		if (response != null && response.length == 1) {
			connectStatusHandler(true);
			var responses = response[0].childNodes;    
			for (var i = 0; i < responses.length; i++) {
				var responseElement = responses[i];

				
				if (responseElement.nodeType != 1) continue;

				var id = responseElement.getAttribute('id');

				var handler = messageHandlers[id];

				if (logging && handler == null) {
					adapter.log('No handler found to match message with id = ' + id);
					continue;
				}

				
				for (var j = 0; j < responseElement.childNodes.length; j++) {
					handler(responseElement.childNodes[j]);
				}
			}
		}
	};

	var errorHandler = function(xhr, status, ex) {
		connectStatusHandler(false);
		if (logging) adapter.log('Error occurred in ajax call. HTTP result: ' +
		                         xhr.status + ', status: ' + status);
	}

	var pollErrorHandler = function(xhr, status, ex) {
		connectStatusHandler(false);
		if (status === 'error' && xhr.status === 0) {
			if (logging) adapter.log('Server connection dropped.');
			setTimeout(function() { sendPoll(); }, pollErrorDelay);
			return;
		}
		if (logging) adapter.log('Error occurred in poll. HTTP result: ' +
		                         xhr.status + ', status: ' + status);
		setTimeout(function() { sendPoll(); }, pollErrorDelay);
	}

	var pollHandler = function(data) {
		try {
			messageHandler(data);
		} catch(e) {
			if (logging) adapter.log('Exception in the poll handler: ' + data, e);
			throw(e);
		} finally {
			setTimeout(sendPoll, pollDelay);
		}
	};

	var initHandler = function(data) {
		sessionInitialized = true;
		if(sessionInitializedCallback) {
			sessionInitializedCallback();
		}
		sendPoll();
	}

	var sendPoll = function() {
		
		
		var now = new Date();
		var timeoutArg = sessionInitialized ? timeout : 0.001;
		var data = 'timeout=' + timeoutArg * 1000
				 + '&d=' + now.getTime()
				 + '&r=' + Math.random();
		var successCallback = sessionInitialized ? pollHandler : initHandler;

		var options = { method: 'get',
			data: addClientId( data ),
			success: successCallback,
			error: pollErrorHandler};
		adapter.ajax(uri, options);
	};

	var sendJmsMessage = function(destination, message, type, headers) {
		var message = {
			destination: destination,
			message: message,
			messageType: type
		};
		
		if (batchInProgress) {
			messageQueue[messageQueue.length] = {message:message, headers:headers};
		} else {
			org.activemq.Amq.startBatch();
			adapter.ajax(uri, { method: 'post',
				data: addClientId( buildParams( [message] ) ),
				error: errorHandler,
				headers: headers,
				success: org.activemq.Amq.endBatch});
		}
	};

	var buildParams = function(msgs) {
		var s = [];
		for (var i = 0, c = msgs.length; i < c; i++) {
			if (i != 0) s[s.length] = '&';
			s[s.length] = ((i == 0) ? 'destination' : 'd' + i);
			s[s.length] = '=';
			s[s.length] = msgs[i].destination;
			s[s.length] = ((i == 0) ? '&message' : '&m' + i);
			s[s.length] = '=';
			s[s.length] = msgs[i].message;
			s[s.length] = ((i == 0) ? '&type' : '&t' + i);
			s[s.length] = '=';
			s[s.length] = msgs[i].messageType;
		}
		return s.join('');
	}
	
	
	var addClientId = function( data ) {
		var output = data || '';
		if( clientId ) {
			if( output.length > 0 ) {
				output += '&';
			}
			output += 'clientId='+clientId;
		}
		return output;
	}

	return {
		
		init : function(options) {
			connectStatusHandler = options.connectStatusHandler || function(connected){};
			uri = options.uri || '/amq';
			pollDelay = typeof options.pollDelay == 'number' ? options.pollDelay : 0;
			timeout = typeof options.timeout == 'number' ? options.timeout : 25;
			logging = options.logging;
			sessionInitializedCallback = options.sessionInitializedCallback
			clientId = options.clientId;
			adapter.init(options);
			sendPoll();
			
		},
		    
		startBatch : function() {
			batchInProgress = true;
		},

		endBatch : function() {
			if (messageQueue.length > 0) {
				var messagesToSend = [];
				var messagesToQueue = [];
				var outgoingHeaders = null;
				
				
				
				
				for(i=0;i<messageQueue.length;i++) {
					
					if ( messageQueue[ i ].headers && messagesToSend.length == 0 ) {
						messagesToSend[ messagesToSend.length ] = messageQueue[ i ].message;
						outgoingHeaders = messageQueue[ i ].headers;
					} else if ( ! messageQueue[ i ].headers && ! outgoingHeaders ) {
						messagesToSend[ messagesToSend.length ] = messageQueue[ i ].message;
					} else {
						messagesToQueue[ messagesToQueue.length ] = messageQueue[ i ];
					}
				}
				var body = buildParams(messagesToSend);
				messageQueue = messagesToQueue;
				org.activemq.Amq.startBatch();
				adapter.ajax(uri, {
					method: 'post',
					headers: outgoingHeaders,
					data: addClientId( body ),
					success: org.activemq.Amq.endBatch, 
					error: errorHandler});
			} else {
				batchInProgress = false;
			}
		},

		
		
		sendMessage : function(destination, message) {
			sendJmsMessage(destination, message, 'send');
		},

		
		
		
		
		
		
		
		
		addListener : function(id, destination, handler, options) {
			messageHandlers[id] = handler;
			var headers = options && options.selector ? {selector:options.selector} : null;
			sendJmsMessage(destination, id, 'listen', headers);
		},

		
		removeListener : function(id, destination) {
			messageHandlers[id] = null;
			sendJmsMessage(destination, id, 'unlisten');
		},
		
		
		getMessageQueue: function() {
			return messageQueue;
		},
		testPollHandler: function( data ) {
			return pollHandler( data );
		}
	};
}();
