

var GLib = imports.gi.GLib;

var Gtk = imports.gi.Gtk;

var Gdk = imports.gi.Gdk;

var WebKit = imports.gi.WebKit;



var startPage = "http:





const DBus = imports.dbus;

function VLCController() { 
    this._init();
};
VLCController.prototype = {
    _init: function() {
	DBus.session.proxifyObject (this,
                   'com.transtep.camera',
                   '/com/transtep/camera');
    }
};
DBus.proxifyPrototype(VLCController.prototype, 
{   
    name: 'com.transtep.camera',
    methods: [
        { name: 'IsCapturing', inSignature: 's', outSignature: 'b'},
        { name: 'StartCapture', inSignature: 's', outSignature: ''},
        { name: 'GetVideoSize', inSignature: 's', outSignature: 'uu'},
        { name: 'List', inSignature: '', outSignature: 'a{su}'},
        { name: 'GetFrame', inSignature: 'ss', outSignature: 'ay'},
        { name: 'stop', inSignature: '', outSignature: ''},
        { name: 'GetFrameAsBase64Content', inSignature: 'ss', outSignature: 's'},
        { name: 'StopCapture', inSignature: 's', outSignature: ''},
        { name: 'StartMonitor', inSignature: 'ss', outSignature: ''},
        { name: 'SetVisual', inSignature: 'ss', outSignature: ''}
    ],
    signals: [
        { name: 'Event', inSignature: 'ss', outSignature: '' }
    ]
});


function Player(vlc, view, screen) {
	var g = view.get_window();
	this.view = view;
	this.vlc = vlc;
	this.wid = g.get_xid()+'';
	print("The Media window ID:" + this.wid); 

/*	if (typeof Seed != 'undefined') {
	    var GLib = imports.gi.GLib;
	    var mainloop = GLib.main_loop_new();
	    GLib.main_loop_run(mainloop);
	}
	else {
	    var Mainloop = imports.mainloop;
	    Mainloop.run('');
	}

	print("Player Initialize [OK]");	
	this.newWindow = function(){
		var player = new Gtk.Window();
		player.resize(w, h);
		player.show_all();
		player.opacity=opacity; 
		player.set_keep_above(1);
		player.set_decorated (false);
	}
*/
};

Player.prototype = {
	view : false,
	vlc : false,
	wid : false,
	url : false,
	handler : function(event, data){
		print("Player Event: " + event);
		var params = new Array(2);
		params[0] = event;
		params[1] = data;
		web.event(this, params);
	},
	getframe : function(f){
		if (this.url)
			this.vlc.GetFrameRemote(this.url, 'png', f);
	},
	getbase64frame : function(f){
		if (this.url)
			this.vlc.GetFrameAsBase64ContentRemote(this.url, 'png', f);
	},
	play : function(url){
		this.url = url;
		print ("url:" + this.url);
		this.vlc.StartMonitorRemote(this.url, this.wid);
	},
	stop : function(){
		if (this.url){
			this.vlc.StopCaptureRemote(this.url);
		}
	}
};


function WebController(view, container){
	this.view = view;
	this.container = container;
	
	view.connect("notify::load-status", function() {
		if (web.view.get_uri() == startPage && web.view.loadStatus == web.WEBKIT_LOAD_FINISHED) {
			
			web.init(); 
		}
	});
}

WebController.prototype = {
	handler : false,
	init : function(){
		
		this.vlc.connect('Event', 
		    function(emitter, id, event) {
			
			web.player.handler(event); 
		    }
		);

		
		this.view.connect("title-changed", function(widget, frame, title) {
			
			web.receiver(title); 
		});

		this.view.execute_script('$.GJS.MPlayerResize()');
	},
	receiver: function(jsonstring){

		print("WebController(receive): " + jsonstring);

		var realObj = JSON.parse(jsonstring);

		if (typeof realObj != 'undefined'){
			if (realObj.Commander == "VideoResize"){
				this.playerArea = new Gtk.DrawingArea();
				this.playerArea.set_size_request(realObj.width, realObj.height);
				this.container.put(this.playerArea, realObj.left, realObj.top);
				this.container.show_all();
				
				this.player = new Player(this.vlc, this.playerArea, 0);
				
				return;
			}else if (realObj.Commander == "VideoHide"){
				this.playerArea.hide();
				return;
			}else if (realObj.Commander == "VideoShow"){
				this.playerArea.show();
				return;
			}else if (realObj.Commander == "VideoPlay"){
				this.player.play(realObj.url);
				return;
			}else if (realObj.Commander == "VideoStop"){
				this.player.stop();
				return;
			}
		}
			
		
		if (typeof this.handler == 'function'){
			this.response(this.handler(realObj), realObj);	
		}

	},
	response : function(jsonstring, realObj){
		print("WebController(response): " + jsonstring);
		if (realObj.FunctionReferenceID!=""){
			print("response callback(#ID):" & realObj.FunctionReferenceID);
			this.view.execute_script("$.GJS.RunAndRemoveFunction('"+realObj.FunctionReferenceID+"','"+jsonstring+"');");
		}
	},
	send : function(data){
		this.response(data);
	},
	event : function(sender, eventargs){
		this.view.execute_script("$.GJS.TiggerEvents('" + JSON.stringify(sender) + "','" + JSON.stringify(eventargs) + "')");
	},
	view : false,
	vlc : new VLCController(),
	player : false,
	WEBKIT_LOAD_PROVISIONAL: 0,
	WEBKIT_LOAD_COMMITTED: 1,
	WEBKIT_LOAD_FINISHED: 2,
	WEBKIT_LOAD_FIRST_VISUALLY_NON_EMPTY_LAYOUT: 3,
	WEBKIT_LOAD_FAILED: 4
}



GLib.set_prgname('KIOSK Player');
Gtk.init(0, null);


var panelMain = new Gtk.Fixed();
var viewWeb = new WebKit.WebView();


var web = new WebController(viewWeb, panelMain);


var windowMain = new Gtk.Window();

windowMain.connect("destroy", function(){
	print("Exit.");
	web.player.stop();
	Gtk.main_quit();
});

/* window event
mediaView.connect("show", function(w,d) {
mediaView.connect("size-request", function(w,r,d) {
mediaView.connect("size-allocate", function(w,a,d) {
mediaView.connect("map", function(w,d) {
*/

windowMain.connect("size-allocate", function(w, a){
	
	viewWeb.set_size_request(a['width'], a['height']);
});



windowMain.resize(1024,768);
windowMain.add(panelMain);
panelMain.put(viewWeb, 0, 0);
viewWeb.load_html_string('<html><meta http-equiv="refresh" content="1; url='+ startPage +'" /></html>', "file:

windowMain.show_all();




web.handler = function(json){
	return JSON.stringify(json);
}




Gtk.main();




