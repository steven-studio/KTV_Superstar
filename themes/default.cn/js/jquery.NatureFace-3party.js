/*!
 * iScroll v4.2.4 ~ Copyright (c) 2012 Matteo Spinelli, http:
 * Released under MIT license, http:
 */
(function(window, doc){
var m = Math,
	dummyStyle = doc.createElement('div').style,
	vendor = (function () {
		var vendors = 't,webkitT,MozT,msT,OT'.split(','),
			t,
			i = 0,
			l = vendors.length;

		for ( ; i < l; i++ ) {
			t = vendors[i] + 'ransform';
			if ( t in dummyStyle ) {
				return vendors[i].substr(0, vendors[i].length - 1);
			}
		}

		return false;
	})(),
	cssVendor = vendor ? '-' + vendor.toLowerCase() + '-' : '',

	
	transform = prefixStyle('transform'),
	transitionProperty = prefixStyle('transitionProperty'),
	transitionDuration = prefixStyle('transitionDuration'),
	transformOrigin = prefixStyle('transformOrigin'),
	transitionTimingFunction = prefixStyle('transitionTimingFunction'),
	transitionDelay = prefixStyle('transitionDelay'),

    
	isAndroid = (/android/gi).test(navigator.appVersion),
	isIDevice = (/iphone|ipad/gi).test(navigator.appVersion),
	isTouchPad = (/hp-tablet/gi).test(navigator.appVersion),

    has3d = prefixStyle('perspective') in dummyStyle,
    hasTouch = 'ontouchstart' in window && !isTouchPad,
    hasTransform = vendor !== false,
    hasTransitionEnd = prefixStyle('transition') in dummyStyle,

	RESIZE_EV = 'onorientationchange' in window ? 'orientationchange' : 'resize',
	START_EV = hasTouch ? 'touchstart' : 'mousedown',
	MOVE_EV = hasTouch ? 'touchmove' : 'mousemove',
	END_EV = hasTouch ? 'touchend' : 'mouseup',
	CANCEL_EV = hasTouch ? 'touchcancel' : 'mouseup',
	TRNEND_EV = (function () {
		if ( vendor === false ) return false;

		var transitionEnd = {
				''			: 'transitionend',
				'webkit'	: 'webkitTransitionEnd',
				'Moz'		: 'transitionend',
				'O'			: 'otransitionend',
				'ms'		: 'MSTransitionEnd'
			};

		return transitionEnd[vendor];
	})(),

	nextFrame = (function() {
		return window.requestAnimationFrame ||
			window.webkitRequestAnimationFrame ||
			window.mozRequestAnimationFrame ||
			window.oRequestAnimationFrame ||
			window.msRequestAnimationFrame ||
			function(callback) { return setTimeout(callback, 1); };
	})(),
	cancelFrame = (function () {
		return window.cancelRequestAnimationFrame ||
			window.webkitCancelAnimationFrame ||
			window.webkitCancelRequestAnimationFrame ||
			window.mozCancelRequestAnimationFrame ||
			window.oCancelRequestAnimationFrame ||
			window.msCancelRequestAnimationFrame ||
			clearTimeout;
	})(),

	
	translateZ = has3d ? ' translateZ(0)' : '',

	
	iScroll = function (el, options) {
		var that = this,
			i;

		that.wrapper = typeof el == 'object' ? el : doc.getElementById(el);
		that.wrapper.style.overflow = 'hidden';
		that.scroller = that.wrapper.children[0];

		
		that.options = {
			hScroll: true,
			vScroll: true,
			x: 0,
			y: 0,
			bounce: true,
			bounceLock: false,
			momentum: true,
			lockDirection: true,
			useTransform: true,
			useTransition: false,
			topOffset: 0,
			checkDOMChanges: false,		
			handleClick: true,

			
			hScrollbar: true,
			vScrollbar: true,
			fixedScrollbar: isAndroid,
			hideScrollbar: isIDevice,
			fadeScrollbar: isIDevice && has3d,
			scrollbarClass: '',

			
			zoom: false,
			zoomMin: 1,
			zoomMax: 4,
			doubleTapZoom: 2,
			wheelAction: 'scroll',

			
			snap: false,
			snapThreshold: 1,

			
			onRefresh: null,
			onBeforeScrollStart: function (e) { e.preventDefault(); },
			onScrollStart: null,
			onBeforeScrollMove: null,
			onScrollMove: null,
			onBeforeScrollEnd: null,
			onScrollEnd: null,
			onTouchEnd: null,
			onDestroy: null,
			onZoomStart: null,
			onZoom: null,
			onZoomEnd: null
		};

		
		for (i in options) that.options[i] = options[i];
		
		
		that.x = that.options.x;
		that.y = that.options.y;

		
		that.options.useTransform = hasTransform && that.options.useTransform;
		that.options.hScrollbar = that.options.hScroll && that.options.hScrollbar;
		that.options.vScrollbar = that.options.vScroll && that.options.vScrollbar;
		that.options.zoom = that.options.useTransform && that.options.zoom;
		that.options.useTransition = hasTransitionEnd && that.options.useTransition;

		
		
		
		if ( that.options.zoom && isAndroid ){
			translateZ = '';
		}
		
		
		that.scroller.style[transitionProperty] = that.options.useTransform ? cssVendor + 'transform' : 'top left';
		that.scroller.style[transitionDuration] = '0';
		that.scroller.style[transformOrigin] = '0 0';
		if (that.options.useTransition) that.scroller.style[transitionTimingFunction] = 'cubic-bezier(0.33,0.66,0.66,1)';
		
		if (that.options.useTransform) that.scroller.style[transform] = 'translate(' + that.x + 'px,' + that.y + 'px)' + translateZ;
		else that.scroller.style.cssText += ';position:absolute;top:' + that.y + 'px;left:' + that.x + 'px';

		if (that.options.useTransition) that.options.fixedScrollbar = true;

		that.refresh();

		that._bind(RESIZE_EV, window);
		that._bind(START_EV);
		if (!hasTouch) {
			if (that.options.wheelAction != 'none') {
				that._bind('DOMMouseScroll');
				that._bind('mousewheel');
			}
		}

		if (that.options.checkDOMChanges) that.checkDOMTime = setInterval(function () {
			that._checkDOMChanges();
		}, 500);
	};


iScroll.prototype = {
	enabled: true,
	x: 0,
	y: 0,
	steps: [],
	scale: 1,
	currPageX: 0, currPageY: 0,
	pagesX: [], pagesY: [],
	aniTime: null,
	wheelZoomCount: 0,
	
	handleEvent: function (e) {
		var that = this;
		switch(e.type) {
			case START_EV:
				if (!hasTouch && e.button !== 0) return;
				that._start(e);
				break;
			case MOVE_EV: that._move(e); break;
			case END_EV:
			case CANCEL_EV: that._end(e); break;
			case RESIZE_EV: that._resize(); break;
			case 'DOMMouseScroll': case 'mousewheel': that._wheel(e); break;
			case TRNEND_EV: that._transitionEnd(e); break;
		}
	},
	
	_checkDOMChanges: function () {
		if (this.moved || this.zoomed || this.animating ||
			(this.scrollerW == this.scroller.offsetWidth * this.scale && this.scrollerH == this.scroller.offsetHeight * this.scale)) return;

		this.refresh();
	},
	
	_scrollbar: function (dir) {
		var that = this,
			bar;

		if (!that[dir + 'Scrollbar']) {
			if (that[dir + 'ScrollbarWrapper']) {
				if (hasTransform) that[dir + 'ScrollbarIndicator'].style[transform] = '';
				that[dir + 'ScrollbarWrapper'].parentNode.removeChild(that[dir + 'ScrollbarWrapper']);
				that[dir + 'ScrollbarWrapper'] = null;
				that[dir + 'ScrollbarIndicator'] = null;
			}

			return;
		}

		if (!that[dir + 'ScrollbarWrapper']) {
			
			bar = doc.createElement('div');

			if (that.options.scrollbarClass) bar.className = that.options.scrollbarClass + dir.toUpperCase();
			else bar.style.cssText = 'position:absolute;z-index:100;' + (dir == 'h' ? 'height:7px;bottom:1px;left:2px;right:' + (that.vScrollbar ? '7' : '2') + 'px' : 'width:7px;bottom:' + (that.hScrollbar ? '7' : '2') + 'px;top:2px;right:1px');

			bar.style.cssText += ';pointer-events:none;' + cssVendor + 'transition-property:opacity;' + cssVendor + 'transition-duration:' + (that.options.fadeScrollbar ? '350ms' : '0') + ';overflow:hidden;opacity:' + (that.options.hideScrollbar ? '0' : '1');

			that.wrapper.appendChild(bar);
			that[dir + 'ScrollbarWrapper'] = bar;

			
			bar = doc.createElement('div');
			if (!that.options.scrollbarClass) {
				bar.style.cssText = 'position:absolute;z-index:100;background:rgba(128,128,128,0.5);border:0px solid rgba(128,128,128,0.5);' + cssVendor + 'background-clip:padding-box;' + cssVendor + 'box-sizing:border-box;' + (dir == 'h' ? 'height:100%' : 'width:100%') + ';' + cssVendor + 'border-radius:3px;border-radius:3px';
			}
			bar.style.cssText += ';pointer-events:none;' + cssVendor + 'transition-property:' + cssVendor + 'transform;' + cssVendor + 'transition-timing-function:cubic-bezier(0.33,0.66,0.66,1);' + cssVendor + 'transition-duration:0;' + cssVendor + 'transform: translate(0,0)' + translateZ;
			if (that.options.useTransition) bar.style.cssText += ';' + cssVendor + 'transition-timing-function:cubic-bezier(0.33,0.66,0.66,1)';

			that[dir + 'ScrollbarWrapper'].appendChild(bar);
			that[dir + 'ScrollbarIndicator'] = bar;
		}

		if (dir == 'h') {
			that.hScrollbarSize = that.hScrollbarWrapper.clientWidth;
			that.hScrollbarIndicatorSize = m.max(m.round(that.hScrollbarSize * that.hScrollbarSize / that.scrollerW), 8);
			that.hScrollbarIndicator.style.width = that.hScrollbarIndicatorSize + 'px';
			that.hScrollbarMaxScroll = that.hScrollbarSize - that.hScrollbarIndicatorSize;
			that.hScrollbarProp = that.hScrollbarMaxScroll / that.maxScrollX;
		} else {
			that.vScrollbarSize = that.vScrollbarWrapper.clientHeight;
			that.vScrollbarIndicatorSize = m.max(m.round(that.vScrollbarSize * that.vScrollbarSize / that.scrollerH), 8);
			that.vScrollbarIndicator.style.height = that.vScrollbarIndicatorSize + 'px';
			that.vScrollbarMaxScroll = that.vScrollbarSize - that.vScrollbarIndicatorSize;
			that.vScrollbarProp = that.vScrollbarMaxScroll / that.maxScrollY;
		}

		
		that._scrollbarPos(dir, true);
	},
	
	_resize: function () {
		var that = this;
		setTimeout(function () { that.refresh(); }, isAndroid ? 200 : 0);
	},
	
	_pos: function (x, y) {
		if (this.zoomed) return;

		x = this.hScroll ? x : 0;
		y = this.vScroll ? y : 0;

		if (this.options.useTransform) {
			this.scroller.style[transform] = 'translate(' + x + 'px,' + y + 'px) scale(' + this.scale + ')' + translateZ;
		} else {
			x = m.round(x);
			y = m.round(y);
			this.scroller.style.left = x + 'px';
			this.scroller.style.top = y + 'px';
		}

		this.x = x;
		this.y = y;

		this._scrollbarPos('h');
		this._scrollbarPos('v');
	},

	_scrollbarPos: function (dir, hidden) {
		var that = this,
			pos = dir == 'h' ? that.x : that.y,
			size;

		if (!that[dir + 'Scrollbar']) return;

		pos = that[dir + 'ScrollbarProp'] * pos;

		if (pos < 0) {
			if (!that.options.fixedScrollbar) {
				size = that[dir + 'ScrollbarIndicatorSize'] + m.round(pos * 3);
				if (size < 8) size = 8;
				that[dir + 'ScrollbarIndicator'].style[dir == 'h' ? 'width' : 'height'] = size + 'px';
			}
			pos = 0;
		} else if (pos > that[dir + 'ScrollbarMaxScroll']) {
			if (!that.options.fixedScrollbar) {
				size = that[dir + 'ScrollbarIndicatorSize'] - m.round((pos - that[dir + 'ScrollbarMaxScroll']) * 3);
				if (size < 8) size = 8;
				that[dir + 'ScrollbarIndicator'].style[dir == 'h' ? 'width' : 'height'] = size + 'px';
				pos = that[dir + 'ScrollbarMaxScroll'] + (that[dir + 'ScrollbarIndicatorSize'] - size);
			} else {
				pos = that[dir + 'ScrollbarMaxScroll'];
			}
		}

		that[dir + 'ScrollbarWrapper'].style[transitionDelay] = '0';
		that[dir + 'ScrollbarWrapper'].style.opacity = hidden && that.options.hideScrollbar ? '0' : '1';
		that[dir + 'ScrollbarIndicator'].style[transform] = 'translate(' + (dir == 'h' ? pos + 'px,0)' : '0,' + pos + 'px)') + translateZ;
	},
	
	_start: function (e) {
		var that = this,
			point = hasTouch ? e.touches[0] : e,
			matrix, x, y,
			c1, c2;

		if (!that.enabled) return;

		if (that.options.onBeforeScrollStart) that.options.onBeforeScrollStart.call(that, e);

		if (that.options.useTransition || that.options.zoom) that._transitionTime(0);

		that.moved = false;
		that.animating = false;
		that.zoomed = false;
		that.distX = 0;
		that.distY = 0;
		that.absDistX = 0;
		that.absDistY = 0;
		that.dirX = 0;
		that.dirY = 0;

		
		if (that.options.zoom && hasTouch && e.touches.length > 1) {
			c1 = m.abs(e.touches[0].pageX-e.touches[1].pageX);
			c2 = m.abs(e.touches[0].pageY-e.touches[1].pageY);
			that.touchesDistStart = m.sqrt(c1 * c1 + c2 * c2);

			that.originX = m.abs(e.touches[0].pageX + e.touches[1].pageX - that.wrapperOffsetLeft * 2) / 2 - that.x;
			that.originY = m.abs(e.touches[0].pageY + e.touches[1].pageY - that.wrapperOffsetTop * 2) / 2 - that.y;

			if (that.options.onZoomStart) that.options.onZoomStart.call(that, e);
		}

		if (that.options.momentum) {
			if (that.options.useTransform) {
				
				matrix = getComputedStyle(that.scroller, null)[transform].replace(/[^0-9\-.,]/g, '').split(',');
				x = +matrix[4];
				y = +matrix[5];
			} else {
				x = +getComputedStyle(that.scroller, null).left.replace(/[^0-9-]/g, '');
				y = +getComputedStyle(that.scroller, null).top.replace(/[^0-9-]/g, '');
			}
			
			if (x != that.x || y != that.y) {
				if (that.options.useTransition) that._unbind(TRNEND_EV);
				else cancelFrame(that.aniTime);
				that.steps = [];
				that._pos(x, y);
				if (that.options.onScrollEnd) that.options.onScrollEnd.call(that);
			}
		}

		that.absStartX = that.x;	
		that.absStartY = that.y;

		that.startX = that.x;
		that.startY = that.y;
		that.pointX = point.pageX;
		that.pointY = point.pageY;

		that.startTime = e.timeStamp || Date.now();

		if (that.options.onScrollStart) that.options.onScrollStart.call(that, e);

		that._bind(MOVE_EV, window);
		that._bind(END_EV, window);
		that._bind(CANCEL_EV, window);
	},
	
	_move: function (e) {
		var that = this,
			point = hasTouch ? e.touches[0] : e,
			deltaX = point.pageX - that.pointX,
			deltaY = point.pageY - that.pointY,
			newX = that.x + deltaX,
			newY = that.y + deltaY,
			c1, c2, scale,
			timestamp = e.timeStamp || Date.now();

		if (that.options.onBeforeScrollMove) that.options.onBeforeScrollMove.call(that, e);

		
		if (that.options.zoom && hasTouch && e.touches.length > 1) {
			c1 = m.abs(e.touches[0].pageX - e.touches[1].pageX);
			c2 = m.abs(e.touches[0].pageY - e.touches[1].pageY);
			that.touchesDist = m.sqrt(c1*c1+c2*c2);

			that.zoomed = true;

			scale = 1 / that.touchesDistStart * that.touchesDist * this.scale;

			if (scale < that.options.zoomMin) scale = 0.5 * that.options.zoomMin * Math.pow(2.0, scale / that.options.zoomMin);
			else if (scale > that.options.zoomMax) scale = 2.0 * that.options.zoomMax * Math.pow(0.5, that.options.zoomMax / scale);

			that.lastScale = scale / this.scale;

			newX = this.originX - this.originX * that.lastScale + this.x,
			newY = this.originY - this.originY * that.lastScale + this.y;

			this.scroller.style[transform] = 'translate(' + newX + 'px,' + newY + 'px) scale(' + scale + ')' + translateZ;

			if (that.options.onZoom) that.options.onZoom.call(that, e);
			return;
		}

		that.pointX = point.pageX;
		that.pointY = point.pageY;

		
		if (newX > 0 || newX < that.maxScrollX) {
			newX = that.options.bounce ? that.x + (deltaX / 2) : newX >= 0 || that.maxScrollX >= 0 ? 0 : that.maxScrollX;
		}
		if (newY > that.minScrollY || newY < that.maxScrollY) {
			newY = that.options.bounce ? that.y + (deltaY / 2) : newY >= that.minScrollY || that.maxScrollY >= 0 ? that.minScrollY : that.maxScrollY;
		}

		that.distX += deltaX;
		that.distY += deltaY;
		that.absDistX = m.abs(that.distX);
		that.absDistY = m.abs(that.distY);

		if (that.absDistX < 6 && that.absDistY < 6) {
			return;
		}

		
		if (that.options.lockDirection) {
			if (that.absDistX > that.absDistY + 5) {
				newY = that.y;
				deltaY = 0;
			} else if (that.absDistY > that.absDistX + 5) {
				newX = that.x;
				deltaX = 0;
			}
		}

		that.moved = true;
		that._pos(newX, newY);
		that.dirX = deltaX > 0 ? -1 : deltaX < 0 ? 1 : 0;
		that.dirY = deltaY > 0 ? -1 : deltaY < 0 ? 1 : 0;

		if (timestamp - that.startTime > 300) {
			that.startTime = timestamp;
			that.startX = that.x;
			that.startY = that.y;
		}
		
		if (that.options.onScrollMove) that.options.onScrollMove.call(that, e);
	},
	
	_end: function (e) {
		if (hasTouch && e.touches.length !== 0) return;

		var that = this,
			point = hasTouch ? e.changedTouches[0] : e,
			target, ev,
			momentumX = { dist:0, time:0 },
			momentumY = { dist:0, time:0 },
			duration = (e.timeStamp || Date.now()) - that.startTime,
			newPosX = that.x,
			newPosY = that.y,
			distX, distY,
			newDuration,
			snap,
			scale;

		that._unbind(MOVE_EV, window);
		that._unbind(END_EV, window);
		that._unbind(CANCEL_EV, window);

		if (that.options.onBeforeScrollEnd) that.options.onBeforeScrollEnd.call(that, e);

		if (that.zoomed) {
			scale = that.scale * that.lastScale;
			scale = Math.max(that.options.zoomMin, scale);
			scale = Math.min(that.options.zoomMax, scale);
			that.lastScale = scale / that.scale;
			that.scale = scale;

			that.x = that.originX - that.originX * that.lastScale + that.x;
			that.y = that.originY - that.originY * that.lastScale + that.y;
			
			that.scroller.style[transitionDuration] = '200ms';
			that.scroller.style[transform] = 'translate(' + that.x + 'px,' + that.y + 'px) scale(' + that.scale + ')' + translateZ;
			
			that.zoomed = false;
			that.refresh();

			if (that.options.onZoomEnd) that.options.onZoomEnd.call(that, e);
			return;
		}

		if (!that.moved) {
			if (hasTouch) {
				if (that.doubleTapTimer && that.options.zoom) {
					
					clearTimeout(that.doubleTapTimer);
					that.doubleTapTimer = null;
					if (that.options.onZoomStart) that.options.onZoomStart.call(that, e);
					that.zoom(that.pointX, that.pointY, that.scale == 1 ? that.options.doubleTapZoom : 1);
					if (that.options.onZoomEnd) {
						setTimeout(function() {
							that.options.onZoomEnd.call(that, e);
						}, 200); 
					}
				} else if (this.options.handleClick) {
					that.doubleTapTimer = setTimeout(function () {
						that.doubleTapTimer = null;

						
						target = point.target;
						while (target.nodeType != 1) target = target.parentNode;

						if (target.tagName != 'SELECT' && target.tagName != 'INPUT' && target.tagName != 'TEXTAREA') {
							ev = doc.createEvent('MouseEvents');
							ev.initMouseEvent('click', true, true, e.view, 1,
								point.screenX, point.screenY, point.clientX, point.clientY,
								e.ctrlKey, e.altKey, e.shiftKey, e.metaKey,
								0, null);
							ev._fake = true;
							target.dispatchEvent(ev);
						}
					}, that.options.zoom ? 250 : 0);
				}
			}

			that._resetPos(400);

			if (that.options.onTouchEnd) that.options.onTouchEnd.call(that, e);
			return;
		}

		if (duration < 300 && that.options.momentum) {
			momentumX = newPosX ? that._momentum(newPosX - that.startX, duration, -that.x, that.scrollerW - that.wrapperW + that.x, that.options.bounce ? that.wrapperW : 0) : momentumX;
			momentumY = newPosY ? that._momentum(newPosY - that.startY, duration, -that.y, (that.maxScrollY < 0 ? that.scrollerH - that.wrapperH + that.y - that.minScrollY : 0), that.options.bounce ? that.wrapperH : 0) : momentumY;

			newPosX = that.x + momentumX.dist;
			newPosY = that.y + momentumY.dist;

			if ((that.x > 0 && newPosX > 0) || (that.x < that.maxScrollX && newPosX < that.maxScrollX)) momentumX = { dist:0, time:0 };
			if ((that.y > that.minScrollY && newPosY > that.minScrollY) || (that.y < that.maxScrollY && newPosY < that.maxScrollY)) momentumY = { dist:0, time:0 };
		}

		if (momentumX.dist || momentumY.dist) {
			newDuration = m.max(m.max(momentumX.time, momentumY.time), 10);

			
			if (that.options.snap) {
				distX = newPosX - that.absStartX;
				distY = newPosY - that.absStartY;
				if (m.abs(distX) < that.options.snapThreshold && m.abs(distY) < that.options.snapThreshold) { that.scrollTo(that.absStartX, that.absStartY, 200); }
				else {
					snap = that._snap(newPosX, newPosY);
					newPosX = snap.x;
					newPosY = snap.y;
					newDuration = m.max(snap.time, newDuration);
				}
			}

			that.scrollTo(m.round(newPosX), m.round(newPosY), newDuration);

			if (that.options.onTouchEnd) that.options.onTouchEnd.call(that, e);
			return;
		}

		
		if (that.options.snap) {
			distX = newPosX - that.absStartX;
			distY = newPosY - that.absStartY;
			if (m.abs(distX) < that.options.snapThreshold && m.abs(distY) < that.options.snapThreshold) that.scrollTo(that.absStartX, that.absStartY, 200);
			else {
				snap = that._snap(that.x, that.y);
				if (snap.x != that.x || snap.y != that.y) that.scrollTo(snap.x, snap.y, snap.time);
			}

			if (that.options.onTouchEnd) that.options.onTouchEnd.call(that, e);
			return;
		}

		that._resetPos(200);
		if (that.options.onTouchEnd) that.options.onTouchEnd.call(that, e);
	},
	
	_resetPos: function (time) {
		var that = this,
			resetX = that.x >= 0 ? 0 : that.x < that.maxScrollX ? that.maxScrollX : that.x,
			resetY = that.y >= that.minScrollY || that.maxScrollY > 0 ? that.minScrollY : that.y < that.maxScrollY ? that.maxScrollY : that.y;

		if (resetX == that.x && resetY == that.y) {
			if (that.moved) {
				that.moved = false;
				if (that.options.onScrollEnd) that.options.onScrollEnd.call(that);		
			}

			if (that.hScrollbar && that.options.hideScrollbar) {
				if (vendor == 'webkit') that.hScrollbarWrapper.style[transitionDelay] = '300ms';
				that.hScrollbarWrapper.style.opacity = '0';
			}
			if (that.vScrollbar && that.options.hideScrollbar) {
				if (vendor == 'webkit') that.vScrollbarWrapper.style[transitionDelay] = '300ms';
				that.vScrollbarWrapper.style.opacity = '0';
			}

			return;
		}

		that.scrollTo(resetX, resetY, time || 0);
	},

	_wheel: function (e) {
		var that = this,
			wheelDeltaX, wheelDeltaY,
			deltaX, deltaY,
			deltaScale;

		if ('wheelDeltaX' in e) {
			wheelDeltaX = e.wheelDeltaX / 12;
			wheelDeltaY = e.wheelDeltaY / 12;
		} else if('wheelDelta' in e) {
			wheelDeltaX = wheelDeltaY = e.wheelDelta / 12;
		} else if ('detail' in e) {
			wheelDeltaX = wheelDeltaY = -e.detail * 3;
		} else {
			return;
		}
		
		if (that.options.wheelAction == 'zoom') {
			deltaScale = that.scale * Math.pow(2, 1/3 * (wheelDeltaY ? wheelDeltaY / Math.abs(wheelDeltaY) : 0));
			if (deltaScale < that.options.zoomMin) deltaScale = that.options.zoomMin;
			if (deltaScale > that.options.zoomMax) deltaScale = that.options.zoomMax;
			
			if (deltaScale != that.scale) {
				if (!that.wheelZoomCount && that.options.onZoomStart) that.options.onZoomStart.call(that, e);
				that.wheelZoomCount++;
				
				that.zoom(e.pageX, e.pageY, deltaScale, 400);
				
				setTimeout(function() {
					that.wheelZoomCount--;
					if (!that.wheelZoomCount && that.options.onZoomEnd) that.options.onZoomEnd.call(that, e);
				}, 400);
			}
			
			return;
		}
		
		deltaX = that.x + wheelDeltaX;
		deltaY = that.y + wheelDeltaY;

		if (deltaX > 0) deltaX = 0;
		else if (deltaX < that.maxScrollX) deltaX = that.maxScrollX;

		if (deltaY > that.minScrollY) deltaY = that.minScrollY;
		else if (deltaY < that.maxScrollY) deltaY = that.maxScrollY;
    
		if (that.maxScrollY < 0) {
			that.scrollTo(deltaX, deltaY, 0);
		}
	},
	
	_transitionEnd: function (e) {
		var that = this;

		if (e.target != that.scroller) return;

		that._unbind(TRNEND_EV);
		
		that._startAni();
	},


	/**
	*
	* Utilities
	*
	*/
	_startAni: function () {
		var that = this,
			startX = that.x, startY = that.y,
			startTime = Date.now(),
			step, easeOut,
			animate;

		if (that.animating) return;
		
		if (!that.steps.length) {
			that._resetPos(400);
			return;
		}
		
		step = that.steps.shift();
		
		if (step.x == startX && step.y == startY) step.time = 0;

		that.animating = true;
		that.moved = true;
		
		if (that.options.useTransition) {
			that._transitionTime(step.time);
			that._pos(step.x, step.y);
			that.animating = false;
			if (step.time) that._bind(TRNEND_EV);
			else that._resetPos(0);
			return;
		}

		animate = function () {
			var now = Date.now(),
				newX, newY;

			if (now >= startTime + step.time) {
				that._pos(step.x, step.y);
				that.animating = false;
				if (that.options.onAnimationEnd) that.options.onAnimationEnd.call(that);			
				that._startAni();
				return;
			}

			now = (now - startTime) / step.time - 1;
			easeOut = m.sqrt(1 - now * now);
			newX = (step.x - startX) * easeOut + startX;
			newY = (step.y - startY) * easeOut + startY;
			that._pos(newX, newY);
			if (that.animating) that.aniTime = nextFrame(animate);
		};

		animate();
	},

	_transitionTime: function (time) {
		time += 'ms';
		this.scroller.style[transitionDuration] = time;
		if (this.hScrollbar) this.hScrollbarIndicator.style[transitionDuration] = time;
		if (this.vScrollbar) this.vScrollbarIndicator.style[transitionDuration] = time;
	},

	_momentum: function (dist, time, maxDistUpper, maxDistLower, size) {
		var deceleration = 0.0006,
			speed = m.abs(dist) / time,
			newDist = (speed * speed) / (2 * deceleration),
			newTime = 0, outsideDist = 0;

		
		if (dist > 0 && newDist > maxDistUpper) {
			outsideDist = size / (6 / (newDist / speed * deceleration));
			maxDistUpper = maxDistUpper + outsideDist;
			speed = speed * maxDistUpper / newDist;
			newDist = maxDistUpper;
		} else if (dist < 0 && newDist > maxDistLower) {
			outsideDist = size / (6 / (newDist / speed * deceleration));
			maxDistLower = maxDistLower + outsideDist;
			speed = speed * maxDistLower / newDist;
			newDist = maxDistLower;
		}

		newDist = newDist * (dist < 0 ? -1 : 1);
		newTime = speed / deceleration;

		return { dist: newDist, time: m.round(newTime) };
	},

	_offset: function (el) {
		var left = -el.offsetLeft,
			top = -el.offsetTop;
			
		while (el = el.offsetParent) {
			left -= el.offsetLeft;
			top -= el.offsetTop;
		}
		
		if (el != this.wrapper) {
			left *= this.scale;
			top *= this.scale;
		}

		return { left: left, top: top };
	},

	_snap: function (x, y) {
		var that = this,
			i, l,
			page, time,
			sizeX, sizeY;

		
		page = that.pagesX.length - 1;
		for (i=0, l=that.pagesX.length; i<l; i++) {
			if (x >= that.pagesX[i]) {
				page = i;
				break;
			}
		}
		if (page == that.currPageX && page > 0 && that.dirX < 0) page--;
		x = that.pagesX[page];
		sizeX = m.abs(x - that.pagesX[that.currPageX]);
		sizeX = sizeX ? m.abs(that.x - x) / sizeX * 500 : 0;
		that.currPageX = page;

		
		page = that.pagesY.length-1;
		for (i=0; i<page; i++) {
			if (y >= that.pagesY[i]) {
				page = i;
				break;
			}
		}
		if (page == that.currPageY && page > 0 && that.dirY < 0) page--;
		y = that.pagesY[page];
		sizeY = m.abs(y - that.pagesY[that.currPageY]);
		sizeY = sizeY ? m.abs(that.y - y) / sizeY * 500 : 0;
		that.currPageY = page;

		
		time = m.round(m.max(sizeX, sizeY)) || 200;

		return { x: x, y: y, time: time };
	},

	_bind: function (type, el, bubble) {
		(el || this.scroller).addEventListener(type, this, !!bubble);
	},

	_unbind: function (type, el, bubble) {
		(el || this.scroller).removeEventListener(type, this, !!bubble);
	},


	/**
	*
	* Public methods
	*
	*/
	destroy: function () {
		var that = this;

		that.scroller.style[transform] = '';

		
		that.hScrollbar = false;
		that.vScrollbar = false;
		that._scrollbar('h');
		that._scrollbar('v');

		
		that._unbind(RESIZE_EV, window);
		that._unbind(START_EV);
		that._unbind(MOVE_EV, window);
		that._unbind(END_EV, window);
		that._unbind(CANCEL_EV, window);
		
		if (!that.options.hasTouch) {
			that._unbind('DOMMouseScroll');
			that._unbind('mousewheel');
		}
		
		if (that.options.useTransition) that._unbind(TRNEND_EV);
		
		if (that.options.checkDOMChanges) clearInterval(that.checkDOMTime);
		
		if (that.options.onDestroy) that.options.onDestroy.call(that);
	},

	refresh: function () {
		var that = this,
			offset,
			i, l,
			els,
			pos = 0,
			page = 0;

		if (that.scale < that.options.zoomMin) that.scale = that.options.zoomMin;
		that.wrapperW = that.wrapper.clientWidth || 1;
		that.wrapperH = that.wrapper.clientHeight || 1;

		that.minScrollY = -that.options.topOffset || 0;
		that.scrollerW = m.round(that.scroller.offsetWidth * that.scale);
		that.scrollerH = m.round((that.scroller.offsetHeight + that.minScrollY) * that.scale);
		that.maxScrollX = that.wrapperW - that.scrollerW;
		that.maxScrollY = that.wrapperH - that.scrollerH + that.minScrollY;
		that.dirX = 0;
		that.dirY = 0;

		if (that.options.onRefresh) that.options.onRefresh.call(that);

		that.hScroll = that.options.hScroll && that.maxScrollX < 0;
		that.vScroll = that.options.vScroll && (!that.options.bounceLock && !that.hScroll || that.scrollerH > that.wrapperH);

		that.hScrollbar = that.hScroll && that.options.hScrollbar;
		that.vScrollbar = that.vScroll && that.options.vScrollbar && that.scrollerH > that.wrapperH;

		offset = that._offset(that.wrapper);
		that.wrapperOffsetLeft = -offset.left;
		that.wrapperOffsetTop = -offset.top;

		
		if (typeof that.options.snap == 'string') {
			that.pagesX = [];
			that.pagesY = [];
			els = that.scroller.querySelectorAll(that.options.snap);
			for (i=0, l=els.length; i<l; i++) {
				pos = that._offset(els[i]);
				pos.left += that.wrapperOffsetLeft;
				pos.top += that.wrapperOffsetTop;
				that.pagesX[i] = pos.left < that.maxScrollX ? that.maxScrollX : pos.left * that.scale;
				that.pagesY[i] = pos.top < that.maxScrollY ? that.maxScrollY : pos.top * that.scale;
			}
		} else if (that.options.snap) {
			that.pagesX = [];
			while (pos >= that.maxScrollX) {
				that.pagesX[page] = pos;
				pos = pos - that.wrapperW;
				page++;
			}
			if (that.maxScrollX%that.wrapperW) that.pagesX[that.pagesX.length] = that.maxScrollX - that.pagesX[that.pagesX.length-1] + that.pagesX[that.pagesX.length-1];

			pos = 0;
			page = 0;
			that.pagesY = [];
			while (pos >= that.maxScrollY) {
				that.pagesY[page] = pos;
				pos = pos - that.wrapperH;
				page++;
			}
			if (that.maxScrollY%that.wrapperH) that.pagesY[that.pagesY.length] = that.maxScrollY - that.pagesY[that.pagesY.length-1] + that.pagesY[that.pagesY.length-1];
		}

		
		that._scrollbar('h');
		that._scrollbar('v');

		if (!that.zoomed) {
			that.scroller.style[transitionDuration] = '0';
			that._resetPos(400);
		}
	},

	scrollTo: function (x, y, time, relative) {
		var that = this,
			step = x,
			i, l;

		that.stop();

		if (!step.length) step = [{ x: x, y: y, time: time, relative: relative }];
		
		for (i=0, l=step.length; i<l; i++) {
			if (step[i].relative) { step[i].x = that.x - step[i].x; step[i].y = that.y - step[i].y; }
			that.steps.push({ x: step[i].x, y: step[i].y, time: step[i].time || 0 });
		}

		that._startAni();
	},

	scrollToElement: function (el, time) {
		var that = this, pos;
		el = el.nodeType ? el : that.scroller.querySelector(el);
		if (!el) return;

		pos = that._offset(el);
		pos.left += that.wrapperOffsetLeft;
		pos.top += that.wrapperOffsetTop;

		pos.left = pos.left > 0 ? 0 : pos.left < that.maxScrollX ? that.maxScrollX : pos.left;
		pos.top = pos.top > that.minScrollY ? that.minScrollY : pos.top < that.maxScrollY ? that.maxScrollY : pos.top;
		time = time === undefined ? m.max(m.abs(pos.left)*2, m.abs(pos.top)*2) : time;

		that.scrollTo(pos.left, pos.top, time);
	},

	scrollToPage: function (pageX, pageY, time) {
		var that = this, x, y;
		
		time = time === undefined ? 400 : time;

		if (that.options.onScrollStart) that.options.onScrollStart.call(that);

		if (that.options.snap) {
			pageX = pageX == 'next' ? that.currPageX+1 : pageX == 'prev' ? that.currPageX-1 : pageX;
			pageY = pageY == 'next' ? that.currPageY+1 : pageY == 'prev' ? that.currPageY-1 : pageY;

			pageX = pageX < 0 ? 0 : pageX > that.pagesX.length-1 ? that.pagesX.length-1 : pageX;
			pageY = pageY < 0 ? 0 : pageY > that.pagesY.length-1 ? that.pagesY.length-1 : pageY;

			that.currPageX = pageX;
			that.currPageY = pageY;
			x = that.pagesX[pageX];
			y = that.pagesY[pageY];
		} else {
			x = -that.wrapperW * pageX;
			y = -that.wrapperH * pageY;
			if (x < that.maxScrollX) x = that.maxScrollX;
			if (y < that.maxScrollY) y = that.maxScrollY;
		}

		that.scrollTo(x, y, time);
	},

	disable: function () {
		this.stop();
		this._resetPos(0);
		this.enabled = false;

		
		this._unbind(MOVE_EV, window);
		this._unbind(END_EV, window);
		this._unbind(CANCEL_EV, window);
	},
	
	enable: function () {
		this.enabled = true;
	},
	
	stop: function () {
		if (this.options.useTransition) this._unbind(TRNEND_EV);
		else cancelFrame(this.aniTime);
		this.steps = [];
		this.moved = false;
		this.animating = false;
	},
	
	zoom: function (x, y, scale, time) {
		var that = this,
			relScale = scale / that.scale;

		if (!that.options.useTransform) return;

		that.zoomed = true;
		time = time === undefined ? 200 : time;
		x = x - that.wrapperOffsetLeft - that.x;
		y = y - that.wrapperOffsetTop - that.y;
		that.x = x - x * relScale + that.x;
		that.y = y - y * relScale + that.y;

		that.scale = scale;
		that.refresh();

		that.x = that.x > 0 ? 0 : that.x < that.maxScrollX ? that.maxScrollX : that.x;
		that.y = that.y > that.minScrollY ? that.minScrollY : that.y < that.maxScrollY ? that.maxScrollY : that.y;

		that.scroller.style[transitionDuration] = time + 'ms';
		that.scroller.style[transform] = 'translate(' + that.x + 'px,' + that.y + 'px) scale(' + scale + ')' + translateZ;
		that.zoomed = false;
	},
	
	isReady: function () {
		return !this.moved && !this.zoomed && !this.animating;
	}
};

function prefixStyle (style) {
	if ( vendor === '' ) return style;

	style = style.charAt(0).toUpperCase() + style.substr(1);
	return vendor + style;
}

dummyStyle = null;	

if (typeof exports !== 'undefined') exports.iScroll = iScroll;
else window.iScroll = iScroll;

})(window, document);
/*!
 * jQuery Cycle Plugin (with Transition Definitions)
 * Examples and documentation at: http:
 * Copyright (c) 2007-2009 M. Alsup
 * Version: 2.74 (03-FEB-2010)
 * Dual licensed under the MIT and GPL licenses:
 * http:
 * http:
 * Requires: jQuery v1.2.6 or later
 */
;(function($) {

var ver = '2.74';


if ($.support == undefined) {
	$.support = {
		opacity: !($.browser.msie)
	};
}

function debug(s) {
	if ($.fn.cycle.debug)
		log(s);
}		
function log() {
	if (window.console && window.console.log)
		window.console.log('[cycle] ' + Array.prototype.join.call(arguments,' '));
};











$.fn.cycle = function(options, arg2) {
	var o = { s: this.selector, c: this.context };

	
	if (this.length === 0 && options != 'stop') {
		if (!$.isReady && o.s) {
			log('DOM not ready, queuing slideshow');
			$(function() {
				$(o.s,o.c).cycle(options,arg2);
			});
			return this;
		}
		
		log('terminating; zero elements found by selector' + ($.isReady ? '' : ' (DOM not ready)'));
		return this;
	}

	
	return this.each(function() {
		var opts = handleArguments(this, options, arg2);
		if (opts === false)
			return;

		
		if (this.cycleTimeout)
			clearTimeout(this.cycleTimeout);
		this.cycleTimeout = this.cyclePause = 0;

		var $cont = $(this);
		var $slides = opts.slideExpr ? $(opts.slideExpr, this) : $cont.children();
		var els = $slides.get();
		if (els.length < 2) {
			log('terminating; too few slides: ' + els.length);
			return;
		}

		var opts2 = buildOptions($cont, $slides, els, opts, o);
		if (opts2 === false)
			return;

		var startTime = opts2.continuous ? 10 : getTimeout(opts2.currSlide, opts2.nextSlide, opts2, !opts2.rev);

		
		if (startTime) {
			startTime += (opts2.delay || 0);
			if (startTime < 10)
				startTime = 10;
			debug('first timeout: ' + startTime);
			this.cycleTimeout = setTimeout(function(){go(els,opts2,0,!opts2.rev)}, startTime);
		}
	});
};


function handleArguments(cont, options, arg2) {
	if (cont.cycleStop == undefined)
		cont.cycleStop = 0;
	if (options === undefined || options === null)
		options = {};
	if (options.constructor == String) {
		switch(options) {
		case 'stop':
			cont.cycleStop++; 
			if (cont.cycleTimeout)
				clearTimeout(cont.cycleTimeout);
			cont.cycleTimeout = 0;
			$(cont).removeData('cycle.opts');
			return false;
		case 'toggle':
			cont.cyclePause = (cont.cyclePause === 1) ? 0 : 1;
			return false;
		case 'pause':
			cont.cyclePause = 1;
			return false;
		case 'resume':
			cont.cyclePause = 0;
			if (arg2 === true) { 
				options = $(cont).data('cycle.opts');
				if (!options) {
					log('options not found, can not resume');
					return false;
				}
				if (cont.cycleTimeout) {
					clearTimeout(cont.cycleTimeout);
					cont.cycleTimeout = 0;
				}
				go(options.elements, options, 1, 1);
			}
			return false;
		case 'prev':
		case 'next':
			var opts = $(cont).data('cycle.opts');
			if (!opts) {
				log('options not found, "prev/next" ignored');
				return false;
			}
			$.fn.cycle[options](opts);
			return false;
		default:
			options = { fx: options };
		};
		return options;
	}
	else if (options.constructor == Number) {
		
		var num = options;
		options = $(cont).data('cycle.opts');
		if (!options) {
			log('options not found, can not advance slide');
			return false;
		}
		if (num < 0 || num >= options.elements.length) {
			log('invalid slide index: ' + num);
			return false;
		}
		options.nextSlide = num;
		if (cont.cycleTimeout) {
			clearTimeout(cont.cycleTimeout);
			cont.cycleTimeout = 0;
		}
		if (typeof arg2 == 'string')
			options.oneTimeFx = arg2;
		go(options.elements, options, 1, num >= options.currSlide);
		return false;
	}
	return options;
};

function removeFilter(el, opts) {
	if (!$.support.opacity && opts.cleartype && el.style.filter) {
		try { el.style.removeAttribute('filter'); }
		catch(smother) {} 
	}
};


function buildOptions($cont, $slides, els, options, o) {
	
	var opts = $.extend({}, $.fn.cycle.defaults, options || {}, $.metadata ? $cont.metadata() : $.meta ? $cont.data() : {});
	if (opts.autostop)
		opts.countdown = opts.autostopCount || els.length;

	var cont = $cont[0];
	$cont.data('cycle.opts', opts);
	opts.$cont = $cont;
	opts.stopCount = cont.cycleStop;
	opts.elements = els;
	opts.before = opts.before ? [opts.before] : [];
	opts.after = opts.after ? [opts.after] : [];
	opts.after.unshift(function(){ opts.busy=0; });

	
	if (!$.support.opacity && opts.cleartype)
		opts.after.push(function() { removeFilter(this, opts); });
	if (opts.continuous)
		opts.after.push(function() { go(els,opts,0,!opts.rev); });

	saveOriginalOpts(opts);

	
	if (!$.support.opacity && opts.cleartype && !opts.cleartypeNoBg)
		clearTypeFix($slides);

	
	if ($cont.css('position') == 'static')
		$cont.css('position', 'relative');
	if (opts.width)
		$cont.width(opts.width);
	if (opts.height && opts.height != 'auto')
		$cont.height(opts.height);

	if (opts.startingSlide)
		opts.startingSlide = parseInt(opts.startingSlide);

	
	if (opts.random) {
		opts.randomMap = [];
		for (var i = 0; i < els.length; i++)
			opts.randomMap.push(i);
		opts.randomMap.sort(function(a,b) {return Math.random() - 0.5;});
		opts.randomIndex = 0;
		opts.startingSlide = opts.randomMap[0];
	}
	else if (opts.startingSlide >= els.length)
		opts.startingSlide = 0; 
	opts.currSlide = opts.startingSlide = opts.startingSlide || 0;
	var first = opts.startingSlide;

	
	$slides.css({position: 'absolute', top:0, left:0}).hide().each(function(i) {
		var z = first ? i >= first ? els.length - (i-first) : first-i : els.length-i;
		$(this).css('z-index', z)
	});

	
	$(els[first]).css('opacity',1).show(); 
	removeFilter(els[first], opts);

	
	if (opts.fit && opts.width)
		$slides.width(opts.width);
	if (opts.fit && opts.height && opts.height != 'auto')
		$slides.height(opts.height);

	
	var reshape = opts.containerResize && !$cont.innerHeight();
	if (reshape) { 
		var maxw = 0, maxh = 0;
		for(var j=0; j < els.length; j++) {
			var $e = $(els[j]), e = $e[0], w = $e.outerWidth(), h = $e.outerHeight();
			if (!w) w = e.offsetWidth;
			if (!h) h = e.offsetHeight;
			maxw = w > maxw ? w : maxw;
			maxh = h > maxh ? h : maxh;
		}
		if (maxw > 0 && maxh > 0)
			$cont.css({width:maxw+'px',height:maxh+'px'});
	}

	if (opts.pause)
		$cont.hover(function(){this.cyclePause++;},function(){this.cyclePause--;});

	if (supportMultiTransitions(opts) === false)
		return false;

	
	
	var requeue = false;
	options.requeueAttempts = options.requeueAttempts || 0;
	$slides.each(function() {
		
		var $el = $(this);
		this.cycleH = (opts.fit && opts.height) ? opts.height : $el.height();
		this.cycleW = (opts.fit && opts.width) ? opts.width : $el.width();

		if ( $el.is('img') ) {
			
			
			
			var loadingIE	= ($.browser.msie  && this.cycleW == 28 && this.cycleH == 30 && !this.complete);
			var loadingFF	= ($.browser.mozilla && this.cycleW == 34 && this.cycleH == 19 && !this.complete);
			var loadingOp	= ($.browser.opera && ((this.cycleW == 42 && this.cycleH == 19) || (this.cycleW == 37 && this.cycleH == 17)) && !this.complete);
			var loadingOther = (this.cycleH == 0 && this.cycleW == 0 && !this.complete);
			
			if (loadingIE || loadingFF || loadingOp || loadingOther) {
				if (o.s && opts.requeueOnImageNotLoaded && ++options.requeueAttempts < 100) { 
					log(options.requeueAttempts,' - img slide not loaded, requeuing slideshow: ', this.src, this.cycleW, this.cycleH);
					setTimeout(function() {$(o.s,o.c).cycle(options)}, opts.requeueTimeout);
					requeue = true;
					return false; 
				}
				else {
					log('could not determine size of image: '+this.src, this.cycleW, this.cycleH);
				}
			}
		}
		return true;
	});

	if (requeue)
		return false;

	opts.cssBefore = opts.cssBefore || {};
	opts.animIn = opts.animIn || {};
	opts.animOut = opts.animOut || {};

	$slides.not(':eq('+first+')').css(opts.cssBefore);
	if (opts.cssFirst)
		$($slides[first]).css(opts.cssFirst);

	if (opts.timeout) {
		opts.timeout = parseInt(opts.timeout);
		
		if (opts.speed.constructor == String)
			opts.speed = $.fx.speeds[opts.speed] || parseInt(opts.speed);
		if (!opts.sync)
			opts.speed = opts.speed / 2;
		while((opts.timeout - opts.speed) < 250) 
			opts.timeout += opts.speed;
	}
	if (opts.easing)
		opts.easeIn = opts.easeOut = opts.easing;
	if (!opts.speedIn)
		opts.speedIn = opts.speed;
	if (!opts.speedOut)
		opts.speedOut = opts.speed;

	opts.slideCount = els.length;
	opts.currSlide = opts.lastSlide = first;
	if (opts.random) {
		opts.nextSlide = opts.currSlide;
		if (++opts.randomIndex == els.length)
			opts.randomIndex = 0;
		opts.nextSlide = opts.randomMap[opts.randomIndex];
	}
	else
		opts.nextSlide = opts.startingSlide >= (els.length-1) ? 0 : opts.startingSlide+1;

	
	if (!opts.multiFx) {
		var init = $.fn.cycle.transitions[opts.fx];
		if ($.isFunction(init))
			init($cont, $slides, opts);
		else if (opts.fx != 'custom' && !opts.multiFx) {
			log('unknown transition: ' + opts.fx,'; slideshow terminating');
			return false;
		}
	}

	
	var e0 = $slides[first];
	if (opts.before.length)
		opts.before[0].apply(e0, [e0, e0, opts, true]);
	if (opts.after.length > 1)
		opts.after[1].apply(e0, [e0, e0, opts, true]);

	if (opts.next)
		$(opts.next).bind(opts.prevNextEvent,function(){return advance(opts,opts.rev?-1:1)});
	if (opts.prev)
		$(opts.prev).bind(opts.prevNextEvent,function(){return advance(opts,opts.rev?1:-1)});
	if (opts.pager)
		buildPager(els,opts);

	exposeAddSlide(opts, els);

	return opts;
};


function saveOriginalOpts(opts) {
	opts.original = { before: [], after: [] };
	opts.original.cssBefore = $.extend({}, opts.cssBefore);
	opts.original.cssAfter  = $.extend({}, opts.cssAfter);
	opts.original.animIn	= $.extend({}, opts.animIn);
	opts.original.animOut   = $.extend({}, opts.animOut);
	$.each(opts.before, function() { opts.original.before.push(this); });
	$.each(opts.after,  function() { opts.original.after.push(this); });
};

function supportMultiTransitions(opts) {
	var i, tx, txs = $.fn.cycle.transitions;
	
	if (opts.fx.indexOf(',') > 0) {
		opts.multiFx = true;
		opts.fxs = opts.fx.replace(/\s*/g,'').split(',');
		
		for (i=0; i < opts.fxs.length; i++) {
			var fx = opts.fxs[i];
			tx = txs[fx];
			if (!tx || !txs.hasOwnProperty(fx) || !$.isFunction(tx)) {
				log('discarding unknown transition: ',fx);
				opts.fxs.splice(i,1);
				i--;
			}
		}
		
		if (!opts.fxs.length) {
			log('No valid transitions named; slideshow terminating.');
			return false;
		}
	}
	else if (opts.fx == 'all') {  
		opts.multiFx = true;
		opts.fxs = [];
		for (p in txs) {
			tx = txs[p];
			if (txs.hasOwnProperty(p) && $.isFunction(tx))
				opts.fxs.push(p);
		}
	}
	if (opts.multiFx && opts.randomizeEffects) {
		
		var r1 = Math.floor(Math.random() * 20) + 30;
		for (i = 0; i < r1; i++) {
			var r2 = Math.floor(Math.random() * opts.fxs.length);
			opts.fxs.push(opts.fxs.splice(r2,1)[0]);
		}
		debug('randomized fx sequence: ',opts.fxs);
	}
	return true;
};


function exposeAddSlide(opts, els) {
	opts.addSlide = function(newSlide, prepend) {
		var $s = $(newSlide), s = $s[0];
		if (!opts.autostopCount)
			opts.countdown++;
		els[prepend?'unshift':'push'](s);
		if (opts.els)
			opts.els[prepend?'unshift':'push'](s); 
		opts.slideCount = els.length;

		$s.css('position','absolute');
		$s[prepend?'prependTo':'appendTo'](opts.$cont);

		if (prepend) {
			opts.currSlide++;
			opts.nextSlide++;
		}

		if (!$.support.opacity && opts.cleartype && !opts.cleartypeNoBg)
			clearTypeFix($s);

		if (opts.fit && opts.width)
			$s.width(opts.width);
		if (opts.fit && opts.height && opts.height != 'auto')
			$slides.height(opts.height);
		s.cycleH = (opts.fit && opts.height) ? opts.height : $s.height();
		s.cycleW = (opts.fit && opts.width) ? opts.width : $s.width();

		$s.css(opts.cssBefore);

		if (opts.pager)
			$.fn.cycle.createPagerAnchor(els.length-1, s, $(opts.pager), els, opts);

		if ($.isFunction(opts.onAddSlide))
			opts.onAddSlide($s);
		else
			$s.hide(); 
	};
}


$.fn.cycle.resetState = function(opts, fx) {
	fx = fx || opts.fx;
	opts.before = []; opts.after = [];
	opts.cssBefore = $.extend({}, opts.original.cssBefore);
	opts.cssAfter  = $.extend({}, opts.original.cssAfter);
	opts.animIn	= $.extend({}, opts.original.animIn);
	opts.animOut   = $.extend({}, opts.original.animOut);
	opts.fxFn = null;
	$.each(opts.original.before, function() { opts.before.push(this); });
	$.each(opts.original.after,  function() { opts.after.push(this); });

	
	var init = $.fn.cycle.transitions[fx];
	if ($.isFunction(init))
		init(opts.$cont, $(opts.elements), opts);
};


function go(els, opts, manual, fwd) {
	
	if (manual && opts.busy && opts.manualTrump) {
		
		$(els).stop(true,true);
		opts.busy = false;
	}
	
	if (opts.busy)
		return;

	var p = opts.$cont[0], curr = els[opts.currSlide], next = els[opts.nextSlide];

	
	if (p.cycleStop != opts.stopCount || p.cycleTimeout === 0 && !manual)
		return;

	
	if (!manual && !p.cyclePause &&
		((opts.autostop && (--opts.countdown <= 0)) ||
		(opts.nowrap && !opts.random && opts.nextSlide < opts.currSlide))) {
		if (opts.end)
			opts.end(opts);
		return;
	}

	
	if (manual || !p.cyclePause) {
		var fx = opts.fx;
		
		curr.cycleH = curr.cycleH || $(curr).height();
		curr.cycleW = curr.cycleW || $(curr).width();
		next.cycleH = next.cycleH || $(next).height();
		next.cycleW = next.cycleW || $(next).width();

		
		if (opts.multiFx) {
			if (opts.lastFx == undefined || ++opts.lastFx >= opts.fxs.length)
				opts.lastFx = 0;
			fx = opts.fxs[opts.lastFx];
			opts.currFx = fx;
		}

		
		if (opts.oneTimeFx) {
			fx = opts.oneTimeFx;
			opts.oneTimeFx = null;
		}

		$.fn.cycle.resetState(opts, fx);

		
		if (opts.before.length)
			$.each(opts.before, function(i,o) {
				if (p.cycleStop != opts.stopCount) return;
				o.apply(next, [curr, next, opts, fwd]);
			});

		
		var after = function() {
			$.each(opts.after, function(i,o) {
				if (p.cycleStop != opts.stopCount) return;
				o.apply(next, [curr, next, opts, fwd]);
			});
		};

		if (opts.nextSlide != opts.currSlide) {
			
			opts.busy = 1;
			if (opts.fxFn) 
				opts.fxFn(curr, next, opts, after, fwd);
			else if ($.isFunction($.fn.cycle[opts.fx])) 
				$.fn.cycle[opts.fx](curr, next, opts, after);
			else
				$.fn.cycle.custom(curr, next, opts, after, manual && opts.fastOnEvent);
		}

		
		opts.lastSlide = opts.currSlide;
		if (opts.random) {
			opts.currSlide = opts.nextSlide;
			if (++opts.randomIndex == els.length)
				opts.randomIndex = 0;
			opts.nextSlide = opts.randomMap[opts.randomIndex];
		}
		else { 
			var roll = (opts.nextSlide + 1) == els.length;
			opts.nextSlide = roll ? 0 : opts.nextSlide+1;
			opts.currSlide = roll ? els.length-1 : opts.nextSlide-1;
		}

		if (opts.pager)
			$.fn.cycle.updateActivePagerLink(opts.pager, opts.currSlide);
	}

	
	var ms = 0;
	if (opts.timeout && !opts.continuous)
		ms = getTimeout(curr, next, opts, fwd);
	else if (opts.continuous && p.cyclePause) 
		ms = 10;
	if (ms > 0)
		p.cycleTimeout = setTimeout(function(){ go(els, opts, 0, !opts.rev) }, ms);
};


$.fn.cycle.updateActivePagerLink = function(pager, currSlide) {
	$(pager).each(function() {
		$(this).find('a').removeClass('activeSlide').filter('a:eq('+currSlide+')').addClass('activeSlide');
	});
};


function getTimeout(curr, next, opts, fwd) {
	if (opts.timeoutFn) {
		
		var t = opts.timeoutFn(curr,next,opts,fwd);
		while ((t - opts.speed) < 250) 
			t += opts.speed;
		debug('calculated timeout: ' + t + '; speed: ' + opts.speed);
		if (t !== false)
			return t;
	}
	return opts.timeout;
};


$.fn.cycle.next = function(opts) { advance(opts, opts.rev?-1:1); };
$.fn.cycle.prev = function(opts) { advance(opts, opts.rev?1:-1);};


function advance(opts, val) {
	var els = opts.elements;
	var p = opts.$cont[0], timeout = p.cycleTimeout;
	if (timeout) {
		clearTimeout(timeout);
		p.cycleTimeout = 0;
	}
	if (opts.random && val < 0) {
		
		opts.randomIndex--;
		if (--opts.randomIndex == -2)
			opts.randomIndex = els.length-2;
		else if (opts.randomIndex == -1)
			opts.randomIndex = els.length-1;
		opts.nextSlide = opts.randomMap[opts.randomIndex];
	}
	else if (opts.random) {
		if (++opts.randomIndex == els.length)
			opts.randomIndex = 0;
		opts.nextSlide = opts.randomMap[opts.randomIndex];
	}
	else {
		opts.nextSlide = opts.currSlide + val;
		if (opts.nextSlide < 0) {
			if (opts.nowrap) return false;
			opts.nextSlide = els.length - 1;
		}
		else if (opts.nextSlide >= els.length) {
			if (opts.nowrap) return false;
			opts.nextSlide = 0;
		}
	}

	if ($.isFunction(opts.prevNextClick))
		opts.prevNextClick(val > 0, opts.nextSlide, els[opts.nextSlide]);
	go(els, opts, 1, val>=0);
	return false;
};

function buildPager(els, opts) {
	var $p = $(opts.pager);
	$.each(els, function(i,o) {
		$.fn.cycle.createPagerAnchor(i,o,$p,els,opts);
	});
   $.fn.cycle.updateActivePagerLink(opts.pager, opts.startingSlide);
};

$.fn.cycle.createPagerAnchor = function(i, el, $p, els, opts) {
	var a;
	if ($.isFunction(opts.pagerAnchorBuilder))
		a = opts.pagerAnchorBuilder(i,el);
	else
		a = '<a href="#">'+(i+1)+'</a>';
		
	if (!a)
		return;
	var $a = $(a);
	
	if ($a.parents('body').length === 0) {
		var arr = [];
		if ($p.length > 1) {
			$p.each(function() {
				var $clone = $a.clone(true);
				$(this).append($clone);
				arr.push($clone[0]);
			});
			$a = $(arr);
		}
		else {
			$a.appendTo($p);
		}
	}

	$a.bind(opts.pagerEvent, function(e) {
		e.preventDefault();
		opts.nextSlide = i;
		var p = opts.$cont[0], timeout = p.cycleTimeout;
		if (timeout) {
			clearTimeout(timeout);
			p.cycleTimeout = 0;
		}
		if ($.isFunction(opts.pagerClick))
			opts.pagerClick(opts.nextSlide, els[opts.nextSlide]);
		go(els,opts,1,opts.currSlide < i); 
		return false;
	});
	
	if (opts.pagerEvent != 'click')
		$a.click(function(){return false;}); 
	
	if (opts.pauseOnPagerHover)
		$a.hover(function() { opts.$cont[0].cyclePause++; }, function() { opts.$cont[0].cyclePause--; } );
};


$.fn.cycle.hopsFromLast = function(opts, fwd) {
	var hops, l = opts.lastSlide, c = opts.currSlide;
	if (fwd)
		hops = c > l ? c - l : opts.slideCount - l;
	else
		hops = c < l ? l - c : l + opts.slideCount - c;
	return hops;
};



function clearTypeFix($slides) {
	function hex(s) {
		s = parseInt(s).toString(16);
		return s.length < 2 ? '0'+s : s;
	};
	function getBg(e) {
		for ( ; e && e.nodeName.toLowerCase() != 'html'; e = e.parentNode) {
			var v = $.css(e,'background-color');
			if (v.indexOf('rgb') >= 0 ) {
				var rgb = v.match(/\d+/g);
				return '#'+ hex(rgb[0]) + hex(rgb[1]) + hex(rgb[2]);
			}
			if (v && v != 'transparent')
				return v;
		}
		return '#ffffff';
	};
	$slides.each(function() { $(this).css('background-color', getBg(this)); });
};


$.fn.cycle.commonReset = function(curr,next,opts,w,h,rev) {
	$(opts.elements).not(curr).hide();
	opts.cssBefore.opacity = 1;
	opts.cssBefore.display = 'block';
	if (w !== false && next.cycleW > 0)
		opts.cssBefore.width = next.cycleW;
	if (h !== false && next.cycleH > 0)
		opts.cssBefore.height = next.cycleH;
	opts.cssAfter = opts.cssAfter || {};
	opts.cssAfter.display = 'none';
	$(curr).css('zIndex',opts.slideCount + (rev === true ? 1 : 0));
	$(next).css('zIndex',opts.slideCount + (rev === true ? 0 : 1));
};


$.fn.cycle.custom = function(curr, next, opts, cb, speedOverride) {
	var $l = $(curr), $n = $(next);
	var speedIn = opts.speedIn, speedOut = opts.speedOut, easeIn = opts.easeIn, easeOut = opts.easeOut;
	$n.css(opts.cssBefore);
	if (speedOverride) {
		if (typeof speedOverride == 'number')
			speedIn = speedOut = speedOverride;
		else
			speedIn = speedOut = 1;
		easeIn = easeOut = null;
	}
	var fn = function() {$n.animate(opts.animIn, speedIn, easeIn, cb)};
	$l.animate(opts.animOut, speedOut, easeOut, function() {
		if (opts.cssAfter) $l.css(opts.cssAfter);
		if (!opts.sync) fn();
	});
	if (opts.sync) fn();
};


$.fn.cycle.transitions = {
	fade: function($cont, $slides, opts) {
		$slides.not(':eq('+opts.currSlide+')').css('opacity',0);
		opts.before.push(function(curr,next,opts) {
			$.fn.cycle.commonReset(curr,next,opts);
			opts.cssBefore.opacity = 0;
		});
		opts.animIn	   = { opacity: 1 };
		opts.animOut   = { opacity: 0 };
		opts.cssBefore = { top: 0, left: 0 };
	}
};

$.fn.cycle.ver = function() { return ver; };


$.fn.cycle.defaults = {
	fx:			  'fade', 
	timeout:	   4000,  
	timeoutFn:     null,  
	continuous:	   0,	  
	speed:		   1000,  
	speedIn:	   null,  
	speedOut:	   null,  
	next:		   null,  
	prev:		   null,  
	prevNextClick: null,  
	prevNextEvent:'click',
	pager:		   null,  
	pagerClick:	   null,  
	pagerEvent:	  'click', 
	pagerAnchorBuilder: null, 
	before:		   null,  
	after:		   null,  
	end:		   null,  
	easing:		   null,  
	easeIn:		   null,  
	easeOut:	   null,  
	shuffle:	   null,  
	animIn:		   null,  
	animOut:	   null,  
	cssBefore:	   null,  
	cssAfter:	   null,  
	fxFn:		   null,  
	height:		  'auto', 
	startingSlide: 0,	  
	sync:		   1,	  
	random:		   0,	  
	fit:		   0,	  
	containerResize: 1,	  
	pause:		   0,	  
	pauseOnPagerHover: 0, 
	autostop:	   0,	  
	autostopCount: 0,	  
	delay:		   0,	  
	slideExpr:	   null,  
	cleartype:	   !$.support.opacity,  
	cleartypeNoBg: false, 
	nowrap:		   0,	  
	fastOnEvent:   0,	  
	randomizeEffects: 1,  
	rev:		   0,	 
	manualTrump:   true,  
	requeueOnImageNotLoaded: true, 
	requeueTimeout: 250   
};

})(jQuery);


/*!
 * jQuery Cycle Plugin Transition Definitions
 * This script is a plugin for the jQuery Cycle Plugin
 * Examples and documentation at: http:
 * Copyright (c) 2007-2008 M. Alsup
 * Version:	 2.72
 * Dual licensed under the MIT and GPL licenses:
 * http:
 * http:
 */
(function($) {






$.fn.cycle.transitions.none = function($cont, $slides, opts) {
	opts.fxFn = function(curr,next,opts,after){
		$(next).show();
		$(curr).hide();
		after();
	};
}


$.fn.cycle.transitions.scrollUp = function($cont, $slides, opts) {
	$cont.css('overflow','hidden');
	opts.before.push($.fn.cycle.commonReset);
	var h = $cont.height();
	opts.cssBefore ={ top: h, left: 0 };
	opts.cssFirst = { top: 0 };
	opts.animIn	  = { top: 0 };
	opts.animOut  = { top: -h };
};
$.fn.cycle.transitions.scrollDown = function($cont, $slides, opts) {
	$cont.css('overflow','hidden');
	opts.before.push($.fn.cycle.commonReset);
	var h = $cont.height();
	opts.cssFirst = { top: 0 };
	opts.cssBefore= { top: -h, left: 0 };
	opts.animIn	  = { top: 0 };
	opts.animOut  = { top: h };
};
$.fn.cycle.transitions.scrollLeft = function($cont, $slides, opts) {
	$cont.css('overflow','hidden');
	opts.before.push($.fn.cycle.commonReset);
	var w = $cont.width();
	opts.cssFirst = { left: 0 };
	opts.cssBefore= { left: w, top: 0 };
	opts.animIn	  = { left: 0 };
	opts.animOut  = { left: 0-w };
};
$.fn.cycle.transitions.scrollRight = function($cont, $slides, opts) {
	$cont.css('overflow','hidden');
	opts.before.push($.fn.cycle.commonReset);
	var w = $cont.width();
	opts.cssFirst = { left: 0 };
	opts.cssBefore= { left: -w, top: 0 };
	opts.animIn	  = { left: 0 };
	opts.animOut  = { left: w };
};
$.fn.cycle.transitions.scrollHorz = function($cont, $slides, opts) {
	$cont.css('overflow','hidden').width();
	opts.before.push(function(curr, next, opts, fwd) {
		$.fn.cycle.commonReset(curr,next,opts);
		opts.cssBefore.left = fwd ? (next.cycleW-1) : (1-next.cycleW);
		opts.animOut.left = fwd ? -curr.cycleW : curr.cycleW;
	});
	opts.cssFirst = { left: 0 };
	opts.cssBefore= { top: 0 };
	opts.animIn   = { left: 0 };
	opts.animOut  = { top: 0 };
};
$.fn.cycle.transitions.scrollVert = function($cont, $slides, opts) {
	$cont.css('overflow','hidden');
	opts.before.push(function(curr, next, opts, fwd) {
		$.fn.cycle.commonReset(curr,next,opts);
		opts.cssBefore.top = fwd ? (1-next.cycleH) : (next.cycleH-1);
		opts.animOut.top = fwd ? curr.cycleH : -curr.cycleH;
	});
	opts.cssFirst = { top: 0 };
	opts.cssBefore= { left: 0 };
	opts.animIn   = { top: 0 };
	opts.animOut  = { left: 0 };
};


$.fn.cycle.transitions.slideX = function($cont, $slides, opts) {
	opts.before.push(function(curr, next, opts) {
		$(opts.elements).not(curr).hide();
		$.fn.cycle.commonReset(curr,next,opts,false,true);
		opts.animIn.width = next.cycleW;
	});
	opts.cssBefore = { left: 0, top: 0, width: 0 };
	opts.animIn	 = { width: 'show' };
	opts.animOut = { width: 0 };
};
$.fn.cycle.transitions.slideY = function($cont, $slides, opts) {
	opts.before.push(function(curr, next, opts) {
		$(opts.elements).not(curr).hide();
		$.fn.cycle.commonReset(curr,next,opts,true,false);
		opts.animIn.height = next.cycleH;
	});
	opts.cssBefore = { left: 0, top: 0, height: 0 };
	opts.animIn	 = { height: 'show' };
	opts.animOut = { height: 0 };
};


$.fn.cycle.transitions.shuffle = function($cont, $slides, opts) {
	var i, w = $cont.css('overflow', 'visible').width();
	$slides.css({left: 0, top: 0});
	opts.before.push(function(curr,next,opts) {
		$.fn.cycle.commonReset(curr,next,opts,true,true,true);
	});
	
	if (!opts.speedAdjusted) {
		opts.speed = opts.speed / 2; 
		opts.speedAdjusted = true;
	}
	opts.random = 0;
	opts.shuffle = opts.shuffle || {left:-w, top:15};
	opts.els = [];
	for (i=0; i < $slides.length; i++)
		opts.els.push($slides[i]);

	for (i=0; i < opts.currSlide; i++)
		opts.els.push(opts.els.shift());

	
	opts.fxFn = function(curr, next, opts, cb, fwd) {
		var $el = fwd ? $(curr) : $(next);
		$(next).css(opts.cssBefore);
		var count = opts.slideCount;
		$el.animate(opts.shuffle, opts.speedIn, opts.easeIn, function() {
			var hops = $.fn.cycle.hopsFromLast(opts, fwd);
			for (var k=0; k < hops; k++)
				fwd ? opts.els.push(opts.els.shift()) : opts.els.unshift(opts.els.pop());
			if (fwd) {
				for (var i=0, len=opts.els.length; i < len; i++)
					$(opts.els[i]).css('z-index', len-i+count);
			}
			else {
				var z = $(curr).css('z-index');
				$el.css('z-index', parseInt(z)+1+count);
			}
			$el.animate({left:0, top:0}, opts.speedOut, opts.easeOut, function() {
				$(fwd ? this : curr).hide();
				if (cb) cb();
			});
		});
	};
	opts.cssBefore = { display: 'block', opacity: 1, top: 0, left: 0 };
};


$.fn.cycle.transitions.turnUp = function($cont, $slides, opts) {
	opts.before.push(function(curr, next, opts) {
		$.fn.cycle.commonReset(curr,next,opts,true,false);
		opts.cssBefore.top = next.cycleH;
		opts.animIn.height = next.cycleH;
	});
	opts.cssFirst  = { top: 0 };
	opts.cssBefore = { left: 0, height: 0 };
	opts.animIn	   = { top: 0 };
	opts.animOut   = { height: 0 };
};
$.fn.cycle.transitions.turnDown = function($cont, $slides, opts) {
	opts.before.push(function(curr, next, opts) {
		$.fn.cycle.commonReset(curr,next,opts,true,false);
		opts.animIn.height = next.cycleH;
		opts.animOut.top   = curr.cycleH;
	});
	opts.cssFirst  = { top: 0 };
	opts.cssBefore = { left: 0, top: 0, height: 0 };
	opts.animOut   = { height: 0 };
};
$.fn.cycle.transitions.turnLeft = function($cont, $slides, opts) {
	opts.before.push(function(curr, next, opts) {
		$.fn.cycle.commonReset(curr,next,opts,false,true);
		opts.cssBefore.left = next.cycleW;
		opts.animIn.width = next.cycleW;
	});
	opts.cssBefore = { top: 0, width: 0  };
	opts.animIn	   = { left: 0 };
	opts.animOut   = { width: 0 };
};
$.fn.cycle.transitions.turnRight = function($cont, $slides, opts) {
	opts.before.push(function(curr, next, opts) {
		$.fn.cycle.commonReset(curr,next,opts,false,true);
		opts.animIn.width = next.cycleW;
		opts.animOut.left = curr.cycleW;
	});
	opts.cssBefore = { top: 0, left: 0, width: 0 };
	opts.animIn	   = { left: 0 };
	opts.animOut   = { width: 0 };
};


$.fn.cycle.transitions.zoom = function($cont, $slides, opts) {
	opts.before.push(function(curr, next, opts) {
		$.fn.cycle.commonReset(curr,next,opts,false,false,true);
		opts.cssBefore.top = next.cycleH/2;
		opts.cssBefore.left = next.cycleW/2;
		opts.animIn	   = { top: 0, left: 0, width: next.cycleW, height: next.cycleH };
		opts.animOut   = { width: 0, height: 0, top: curr.cycleH/2, left: curr.cycleW/2 };
	});
	opts.cssFirst = { top:0, left: 0 };
	opts.cssBefore = { width: 0, height: 0 };
};


$.fn.cycle.transitions.fadeZoom = function($cont, $slides, opts) {
	opts.before.push(function(curr, next, opts) {
		$.fn.cycle.commonReset(curr,next,opts,false,false);
		opts.cssBefore.left = next.cycleW/2;
		opts.cssBefore.top = next.cycleH/2;
		opts.animIn	= { top: 0, left: 0, width: next.cycleW, height: next.cycleH };
	});
	opts.cssBefore = { width: 0, height: 0 };
	opts.animOut  = { opacity: 0 };
};


$.fn.cycle.transitions.blindX = function($cont, $slides, opts) {
	var w = $cont.css('overflow','hidden').width();
	opts.before.push(function(curr, next, opts) {
		$.fn.cycle.commonReset(curr,next,opts);
		opts.animIn.width = next.cycleW;
		opts.animOut.left   = curr.cycleW;
	});
	opts.cssBefore = { left: w, top: 0 };
	opts.animIn = { left: 0 };
	opts.animOut  = { left: w };
};

$.fn.cycle.transitions.blindY = function($cont, $slides, opts) {
	var h = $cont.css('overflow','hidden').height();
	opts.before.push(function(curr, next, opts) {
		$.fn.cycle.commonReset(curr,next,opts);
		opts.animIn.height = next.cycleH;
		opts.animOut.top   = curr.cycleH;
	});
	opts.cssBefore = { top: h, left: 0 };
	opts.animIn = { top: 0 };
	opts.animOut  = { top: h };
};

$.fn.cycle.transitions.blindZ = function($cont, $slides, opts) {
	var h = $cont.css('overflow','hidden').height();
	var w = $cont.width();
	opts.before.push(function(curr, next, opts) {
		$.fn.cycle.commonReset(curr,next,opts);
		opts.animIn.height = next.cycleH;
		opts.animOut.top   = curr.cycleH;
	});
	opts.cssBefore = { top: h, left: w };
	opts.animIn = { top: 0, left: 0 };
	opts.animOut  = { top: h, left: w };
};


$.fn.cycle.transitions.growX = function($cont, $slides, opts) {
	opts.before.push(function(curr, next, opts) {
		$.fn.cycle.commonReset(curr,next,opts,false,true);
		opts.cssBefore.left = this.cycleW/2;
		opts.animIn = { left: 0, width: this.cycleW };
		opts.animOut = { left: 0 };
	});
	opts.cssBefore = { width: 0, top: 0 };
};

$.fn.cycle.transitions.growY = function($cont, $slides, opts) {
	opts.before.push(function(curr, next, opts) {
		$.fn.cycle.commonReset(curr,next,opts,true,false);
		opts.cssBefore.top = this.cycleH/2;
		opts.animIn = { top: 0, height: this.cycleH };
		opts.animOut = { top: 0 };
	});
	opts.cssBefore = { height: 0, left: 0 };
};


$.fn.cycle.transitions.curtainX = function($cont, $slides, opts) {
	opts.before.push(function(curr, next, opts) {
		$.fn.cycle.commonReset(curr,next,opts,false,true,true);
		opts.cssBefore.left = next.cycleW/2;
		opts.animIn = { left: 0, width: this.cycleW };
		opts.animOut = { left: curr.cycleW/2, width: 0 };
	});
	opts.cssBefore = { top: 0, width: 0 };
};

$.fn.cycle.transitions.curtainY = function($cont, $slides, opts) {
	opts.before.push(function(curr, next, opts) {
		$.fn.cycle.commonReset(curr,next,opts,true,false,true);
		opts.cssBefore.top = next.cycleH/2;
		opts.animIn = { top: 0, height: next.cycleH };
		opts.animOut = { top: curr.cycleH/2, height: 0 };
	});
	opts.cssBefore = { left: 0, height: 0 };
};


$.fn.cycle.transitions.cover = function($cont, $slides, opts) {
	var d = opts.direction || 'left';
	var w = $cont.css('overflow','hidden').width();
	var h = $cont.height();
	opts.before.push(function(curr, next, opts) {
		$.fn.cycle.commonReset(curr,next,opts);
		if (d == 'right')
			opts.cssBefore.left = -w;
		else if (d == 'up')
			opts.cssBefore.top = h;
		else if (d == 'down')
			opts.cssBefore.top = -h;
		else
			opts.cssBefore.left = w;
	});
	opts.animIn = { left: 0, top: 0};
	opts.animOut = { opacity: 1 };
	opts.cssBefore = { top: 0, left: 0 };
};


$.fn.cycle.transitions.uncover = function($cont, $slides, opts) {
	var d = opts.direction || 'left';
	var w = $cont.css('overflow','hidden').width();
	var h = $cont.height();
	opts.before.push(function(curr, next, opts) {
		$.fn.cycle.commonReset(curr,next,opts,true,true,true);
		if (d == 'right')
			opts.animOut.left = w;
		else if (d == 'up')
			opts.animOut.top = -h;
		else if (d == 'down')
			opts.animOut.top = h;
		else
			opts.animOut.left = -w;
	});
	opts.animIn = { left: 0, top: 0 };
	opts.animOut = { opacity: 1 };
	opts.cssBefore = { top: 0, left: 0 };
};


$.fn.cycle.transitions.toss = function($cont, $slides, opts) {
	var w = $cont.css('overflow','visible').width();
	var h = $cont.height();
	opts.before.push(function(curr, next, opts) {
		$.fn.cycle.commonReset(curr,next,opts,true,true,true);
		
		if (!opts.animOut.left && !opts.animOut.top)
			opts.animOut = { left: w*2, top: -h/2, opacity: 0 };
		else
			opts.animOut.opacity = 0;
	});
	opts.cssBefore = { left: 0, top: 0 };
	opts.animIn = { left: 0 };
};


$.fn.cycle.transitions.wipe = function($cont, $slides, opts) {
	var w = $cont.css('overflow','hidden').width();
	var h = $cont.height();
	opts.cssBefore = opts.cssBefore || {};
	var clip;
	if (opts.clip) {
		if (/l2r/.test(opts.clip))
			clip = 'rect(0px 0px '+h+'px 0px)';
		else if (/r2l/.test(opts.clip))
			clip = 'rect(0px '+w+'px '+h+'px '+w+'px)';
		else if (/t2b/.test(opts.clip))
			clip = 'rect(0px '+w+'px 0px 0px)';
		else if (/b2t/.test(opts.clip))
			clip = 'rect('+h+'px '+w+'px '+h+'px 0px)';
		else if (/zoom/.test(opts.clip)) {
			var top = parseInt(h/2);
			var left = parseInt(w/2);
			clip = 'rect('+top+'px '+left+'px '+top+'px '+left+'px)';
		}
	}

	opts.cssBefore.clip = opts.cssBefore.clip || clip || 'rect(0px 0px 0px 0px)';

	var d = opts.cssBefore.clip.match(/(\d+)/g);
	var t = parseInt(d[0]), r = parseInt(d[1]), b = parseInt(d[2]), l = parseInt(d[3]);

	opts.before.push(function(curr, next, opts) {
		if (curr == next) return;
		var $curr = $(curr), $next = $(next);
		$.fn.cycle.commonReset(curr,next,opts,true,true,false);
		opts.cssAfter.display = 'block';

		var step = 1, count = parseInt((opts.speedIn / 13)) - 1;
		(function f() {
			var tt = t ? t - parseInt(step * (t/count)) : 0;
			var ll = l ? l - parseInt(step * (l/count)) : 0;
			var bb = b < h ? b + parseInt(step * ((h-b)/count || 1)) : h;
			var rr = r < w ? r + parseInt(step * ((w-r)/count || 1)) : w;
			$next.css({ clip: 'rect('+tt+'px '+rr+'px '+bb+'px '+ll+'px)' });
			(step++ <= count) ? setTimeout(f, 13) : $curr.css('display', 'none');
		})();
	});
	opts.cssBefore = { display: 'block', opacity: 1, top: 0, left: 0 };
	opts.animIn	   = { left: 0 };
	opts.animOut   = { left: 0 };
};

})(jQuery);
﻿/**
 * jQuery.timers - Timer abstractions for jQuery
 * Written by Blair Mitchelmore (blair DOT mitchelmore AT gmail DOT com)
 * Licensed under the WTFPL (http:
 * Date: 2009/10/16
 *
 * @author Blair Mitchelmore
 * @version 1.2
 *
 **/

jQuery.fn.extend({
	everyTime: function(interval, label, fn, times) {
		return this.each(function() {
			jQuery.timer.add(this, interval, label, fn, times);
		});
	},
	oneTime: function(interval, label, fn) {
		return this.each(function() {
			jQuery.timer.add(this, interval, label, fn, 1);
		});
	},
	stopTime: function(label, fn) {
		return this.each(function() {
			jQuery.timer.remove(this, label, fn);
		});
	}
});

jQuery.extend({
	timer: {
		global: [],
		guid: 1,
		dataKey: "jQuery.timer",
		regex: /^([0-9]+(?:\.[0-9]*)?)\s*(.*s)?$/,
		powers: {
			
			'ms': 1,
			'cs': 10,
			'ds': 100,
			's': 1000,
			'das': 10000,
			'hs': 100000,
			'ks': 1000000
		},
		timeParse: function(value) {
			if (value == undefined || value == null)
				return null;
			var result = this.regex.exec(jQuery.trim(value.toString()));
			if (result[2]) {
				var num = parseFloat(result[1]);
				var mult = this.powers[result[2]] || 1;
				return num * mult;
			} else {
				return value;
			}
		},
		add: function(element, interval, label, fn, times) {
			var counter = 0;
			
			if (jQuery.isFunction(label)) {
				if (!times) 
					times = fn;
				fn = label;
				label = interval;
			}
			
			interval = jQuery.timer.timeParse(interval);

			if (typeof interval != 'number' || isNaN(interval) || interval < 0)
				return;

			if (typeof times != 'number' || isNaN(times) || times < 0) 
				times = 0;
			
			times = times || 0;
			
			var timers = jQuery.data(element, this.dataKey) || jQuery.data(element, this.dataKey, {});
			
			if (!timers[label])
				timers[label] = {};
			
			fn.timerID = fn.timerID || this.guid++;
			
			var handler = function() {
				if ((++counter > times && times !== 0) || fn.call(element, counter) === false)
					jQuery.timer.remove(element, label, fn);
			};
			
			handler.timerID = fn.timerID;
			
			if (!timers[label][fn.timerID])
				timers[label][fn.timerID] = window.setInterval(handler,interval);
			
			this.global.push( element );
			
		},
		remove: function(element, label, fn) {
			var timers = jQuery.data(element, this.dataKey), ret;
			
			if ( timers ) {
				
				if (!label) {
					for ( label in timers )
						this.remove(element, label, fn);
				} else if ( timers[label] ) {
					if ( fn ) {
						if ( fn.timerID ) {
							window.clearInterval(timers[label][fn.timerID]);
							delete timers[label][fn.timerID];
						}
					} else {
						for ( var fn in timers[label] ) {
							window.clearInterval(timers[label][fn]);
							delete timers[label][fn];
						}
					}
					
					for ( ret in timers[label] ) break;
					if ( !ret ) {
						ret = null;
						delete timers[label];
					}
				}
				
				for ( ret in timers ) break;
				if ( !ret ) 
					jQuery.removeData(element, this.dataKey);
			}
		}
	}
});

jQuery(window).bind("unload", function() {
	jQuery.each(jQuery.timer.global, function(index, item) {
		jQuery.timer.remove(item);
	});
});
!function(e,t,n){function o(e,n){var r=t.createElement(e||"div"),i;for(i in n)r[i]=n[i];return r}function u(e){for(var t=1,n=arguments.length;t<n;t++)e.appendChild(arguments[t]);return e}function f(e,t,n,r){var o=["opacity",t,~~(e*100),n,r].join("-"),u=.01+n/r*100,f=Math.max(1-(1-e)/t*(100-u),e),l=s.substring(0,s.indexOf("Animation")).toLowerCase(),c=l&&"-"+l+"-"||"";return i[o]||(a.insertRule("@"+c+"keyframes "+o+"{"+"0%{opacity:"+f+"}"+u+"%{opacity:"+e+"}"+(u+.01)+"%{opacity:1}"+(u+t)%100+"%{opacity:"+e+"}"+"100%{opacity:"+f+"}"+"}",a.cssRules.length),i[o]=1),o}function l(e,t){var i=e.style,s,o;if(i[t]!==n)return t;t=t.charAt(0).toUpperCase()+t.slice(1);for(o=0;o<r.length;o++){s=r[o]+t;if(i[s]!==n)return s}}function c(e,t){for(var n in t)e.style[l(e,n)||n]=t[n];return e}function h(e){for(var t=1;t<arguments.length;t++){var r=arguments[t];for(var i in r)e[i]===n&&(e[i]=r[i])}return e}function p(e){var t={x:e.offsetLeft,y:e.offsetTop};while(e=e.offsetParent)t.x+=e.offsetLeft,t.y+=e.offsetTop;return t}var r=["webkit","Moz","ms","O"],i={},s,a=function(){var e=o("style",{type:"text/css"});return u(t.getElementsByTagName("head")[0],e),e.sheet||e.styleSheet}(),d={lines:12,length:7,width:5,radius:10,rotate:0,corners:1,color:"#000",speed:1,trail:100,opacity:.25,fps:20,zIndex:2e9,className:"spinner",top:"auto",left:"auto",position:"relative"},v=function m(e){if(!this.spin)return new m(e);this.opts=h(e||{},m.defaults,d)};v.defaults={},h(v.prototype,{spin:function(e){this.stop();var t=this,n=t.opts,r=t.el=c(o(0,{className:n.className}),{position:n.position,width:0,zIndex:n.zIndex}),i=n.radius+n.length+n.width,u,a;e&&(e.insertBefore(r,e.firstChild||null),a=p(e),u=p(r),c(r,{left:(n.left=="auto"?a.x-u.x+(e.offsetWidth>>1):parseInt(n.left,10)+i)+"px",top:(n.top=="auto"?a.y-u.y+(e.offsetHeight>>1):parseInt(n.top,10)+i)+"px"})),r.setAttribute("aria-role","progressbar"),t.lines(r,t.opts);if(!s){var f=0,l=n.fps,h=l/n.speed,d=(1-n.opacity)/(h*n.trail/100),v=h/n.lines;(function m(){f++;for(var e=n.lines;e;e--){var i=Math.max(1-(f+e*v)%h*d,n.opacity);t.opacity(r,n.lines-e,i,n)}t.timeout=t.el&&setTimeout(m,~~(1e3/l))})()}return t},stop:function(){var e=this.el;return e&&(clearTimeout(this.timeout),e.parentNode&&e.parentNode.removeChild(e),this.el=n),this},lines:function(e,t){function i(e,r){return c(o(),{position:"absolute",width:t.length+t.width+"px",height:t.width+"px",background:e,boxShadow:r,transformOrigin:"left",transform:"rotate("+~~(360/t.lines*n+t.rotate)+"deg) translate("+t.radius+"px"+",0)",borderRadius:(t.corners*t.width>>1)+"px"})}var n=0,r;for(;n<t.lines;n++)r=c(o(),{position:"absolute",top:1+~(t.width/2)+"px",transform:t.hwaccel?"translate3d(0,0,0)":"",opacity:t.opacity,animation:s&&f(t.opacity,t.trail,n,t.lines)+" "+1/t.speed+"s linear infinite"}),t.shadow&&u(r,c(i("#000","0 0 4px #000"),{top:"2px"})),u(e,u(r,i(t.color,"0 0 1px rgba(0,0,0,.1)")));return e},opacity:function(e,t,n){t<e.childNodes.length&&(e.childNodes[t].style.opacity=n)}}),function(){function e(e,t){return o("<"+e+' xmlns="urn:schemas-microsoft.com:vml" class="spin-vml">',t)}var t=c(o("group"),{behavior:"url(#default#VML)"});!l(t,"transform")&&t.adj?(a.addRule(".spin-vml","behavior:url(#default#VML)"),v.prototype.lines=function(t,n){function s(){return c(e("group",{coordsize:i+" "+i,coordorigin:-r+" "+ -r}),{width:i,height:i})}function l(t,i,o){u(a,u(c(s(),{rotation:360/n.lines*t+"deg",left:~~i}),u(c(e("roundrect",{arcsize:n.corners}),{width:r,height:n.width,left:n.radius,top:-n.width>>1,filter:o}),e("fill",{color:n.color,opacity:n.opacity}),e("stroke",{opacity:0}))))}var r=n.length+n.width,i=2*r,o=-(n.width+n.length)*2+"px",a=c(s(),{position:"absolute",top:o,left:o}),f;if(n.shadow)for(f=1;f<=n.lines;f++)l(f,-2,"progid:DXImageTransform.Microsoft.Blur(pixelradius=2,makeshadow=1,shadowopacity=.3)");for(f=1;f<=n.lines;f++)l(f);return u(t,a)},v.prototype.opacity=function(e,t,n,r){var i=e.firstChild;r=r.shadow&&r.lines||0,i&&t+r<i.childNodes.length&&(i=i.childNodes[t+r],i=i&&i.firstChild,i=i&&i.firstChild,i&&(i.opacity=n))}):s=l(t,"animation")}(),typeof define=="function"&&define.amd?define(function(){return v}):e.Spinner=v}(window,document);/**
 * Flash (http:
 * A jQuery plugin for embedding Flash movies.
 * 
 * Version 1.0
 * November 9th, 2006
 *
 * Copyright (c) 2006 Luke Lutman (http:
 * Dual licensed under the MIT and GPL licenses.
 * http:
 * http:
 * 
 * Inspired by:
 * SWFObject (http:
 * UFO (http:
 * sIFR (http:
 * 
 * IMPORTANT: 
 * The packed version of jQuery breaks ActiveX control
 * activation in Internet Explorer. Use JSMin to minifiy
 * jQuery (see: http:
 *
 **/ 
;(function(){
	
var $$;

/**
 * 
 * @desc Replace matching elements with a flash movie.
 * @author Luke Lutman
 * @version 1.0.1
 *
 * @name flash
 * @param Hash htmlOptions Options for the embed/object tag.
 * @param Hash pluginOptions Options for detecting/updating the Flash plugin (optional).
 * @param Function replace Custom block called for each matched element if flash is installed (optional).
 * @param Function update Custom block called for each matched if flash isn't installed (optional).
 * @type jQuery
 *
 * @cat plugins/flash
 * 
 * @example $('#hello').flash({ src: 'hello.swf' });
 * @desc Embed a Flash movie.
 *
 * @example $('#hello').flash({ src: 'hello.swf' }, { version: 8 });
 * @desc Embed a Flash 8 movie.
 *
 * @example $('#hello').flash({ src: 'hello.swf' }, { expressInstall: true });
 * @desc Embed a Flash movie using Express Install if flash isn't installed.
 *
 * @example $('#hello').flash({ src: 'hello.swf' }, { update: false });
 * @desc Embed a Flash movie, don't show an update message if Flash isn't installed.
 *
**/
$$ = jQuery.fn.flash = function(htmlOptions, pluginOptions, replace, update) {
	
	
	var block = replace || $$.replace;
	
	
	pluginOptions = $$.copy($$.pluginOptions, pluginOptions);
	
	
	if(!$$.hasFlash(pluginOptions.version)) {
		
		if(pluginOptions.expressInstall && $$.hasFlash(6,0,65)) {
			
			var expressInstallOptions = {
				flashvars: {  	
					MMredirectURL: location,
					MMplayerType: 'PlugIn',
					MMdoctitle: jQuery('title').text() 
				}					
			};
		
		} else if (pluginOptions.update) {
			
			block = update || $$.update;
		
		} else {
			
			
			
			
			return this;
		}
	}
	
	
	htmlOptions = $$.copy($$.htmlOptions, expressInstallOptions, htmlOptions);
	
	
	return this.each(function(){
		block.call(this, $$.copy(htmlOptions));
	});
	
};
/**
 *
 * @name flash.copy
 * @desc Copy an arbitrary number of objects into a new object.
 * @type Object
 * 
 * @example $$.copy({ foo: 1 }, { bar: 2 });
 * @result { foo: 1, bar: 2 };
 *
**/
$$.copy = function() {
	var options = {}, flashvars = {};
	for(var i = 0; i < arguments.length; i++) {
		var arg = arguments[i];
		if(arg == undefined) continue;
		jQuery.extend(options, arg);
		
		
		if(arg.flashvars == undefined) continue;
		jQuery.extend(flashvars, arg.flashvars);
	}
	options.flashvars = flashvars;
	return options;
};
/*
 * @name flash.hasFlash
 * @desc Check if a specific version of the Flash plugin is installed
 * @type Boolean
 *
**/
$$.hasFlash = function() {
	
	if(/hasFlash\=true/.test(location)) return true;
	if(/hasFlash\=false/.test(location)) return false;
	var pv = $$.hasFlash.playerVersion().match(/\d+/g);
	var rv = String([arguments[0], arguments[1], arguments[2]]).match(/\d+/g) || String($$.pluginOptions.version).match(/\d+/g);
	for(var i = 0; i < 3; i++) {
		pv[i] = parseInt(pv[i] || 0);
		rv[i] = parseInt(rv[i] || 0);
		
		if(pv[i] < rv[i]) return false;
		
		if(pv[i] > rv[i]) return true;
	}
	
	return true;
};
/**
 *
 * @name flash.hasFlash.playerVersion
 * @desc Get the version of the installed Flash plugin.
 * @type String
 *
**/
$$.hasFlash.playerVersion = function() {
	
	try {
		try {
			
			
			var axo = new ActiveXObject('ShockwaveFlash.ShockwaveFlash.6');
			try { axo.AllowScriptAccess = 'always';	} 
			catch(e) { return '6,0,0'; }				
		} catch(e) {}
		return new ActiveXObject('ShockwaveFlash.ShockwaveFlash').GetVariable('$version').replace(/\D+/g, ',').match(/^,?(.+),?$/)[1];
	
	} catch(e) {
		try {
			if(navigator.mimeTypes["application/x-shockwave-flash"].enabledPlugin){
				return (navigator.plugins["Shockwave Flash 2.0"] || navigator.plugins["Shockwave Flash"]).description.replace(/\D+/g, ",").match(/^,?(.+),?$/)[1];
			}
		} catch(e) {}		
	}
	return '0,0,0';
};
/**
 *
 * @name flash.htmlOptions
 * @desc The default set of options for the object or embed tag.
 *
**/
$$.htmlOptions = {
	height: 240,
	flashvars: {},
	pluginspage: 'http:
	src: '#',
	type: 'application/x-shockwave-flash',
	width: 320		
};
/**
 *
 * @name flash.pluginOptions
 * @desc The default set of options for checking/updating the flash Plugin.
 *
**/
$$.pluginOptions = {
	expressInstall: false,
	update: true,
	version: '6.0.65'
};
/**
 *
 * @name flash.replace
 * @desc The default method for replacing an element with a Flash movie.
 *
**/
$$.replace = function(htmlOptions) {
	this.innerHTML = '<div class="alt">'+this.innerHTML+'</div>';
	jQuery(this)
		.addClass('flash-replaced')
		.prepend($$.transform(htmlOptions));
};
/**
 *
 * @name flash.update
 * @desc The default method for replacing an element with an update message.
 *
**/
$$.update = function(htmlOptions) {
	var url = String(location).split('?');
	url.splice(1,0,'?hasFlash=true&');
	url = url.join('');
	var msg = '<p>This content requires the Flash Player. <a href="http:
	this.innerHTML = '<span class="alt">'+this.innerHTML+'</span>';
	jQuery(this)
		.addClass('flash-update')
		.prepend(msg);
};
/**
 *
 * @desc Convert a hash of html options to a string of attributes, using Function.apply(). 
 * @example toAttributeString.apply(htmlOptions)
 * @result foo="bar" foo="bar"
 *
**/
function toAttributeString() {
	var s = '';
	for(var key in this)
		if(typeof this[key] != 'function')
			s += key+'="'+this[key]+'" ';
	return s;		
};
/**
 *
 * @desc Convert a hash of flashvars to a url-encoded string, using Function.apply(). 
 * @example toFlashvarsString.apply(flashvarsObject)
 * @result foo=bar&foo=bar
 *
**/
function toFlashvarsString() {
	var s = '';
	for(var key in this)
		if(typeof this[key] != 'function')
			s += key+'='+encodeURIComponent(this[key])+'&';
	return s.replace(/&$/, '');		
};
/**
 *
 * @name flash.transform
 * @desc Transform a set of html options into an embed tag.
 * @type String 
 *
 * @example $$.transform(htmlOptions)
 * @result <embed src="foo.swf" ... />
 *
 * Note: The embed tag is NOT standards-compliant, but it 
 * works in all current browsers. flash.transform can be
 * overwritten with a custom function to generate more 
 * standards-compliant markup.
 *
**/
$$.transform = function(htmlOptions) {
	htmlOptions.toString = toAttributeString;
	if(htmlOptions.flashvars) htmlOptions.flashvars.toString = toFlashvarsString;
	return '<embed ' + String(htmlOptions) + '/>';		
};

/**
 *
 * Flash Player 9 Fix (http:
 *
**/
if (window.attachEvent) {
	window.attachEvent("onbeforeunload", function(){
		__flash_unloadHandler = function() {};
		__flash_savedUnloadHandler = function() {};
	});
}
	
})();
/**
* author Remy Sharp
* url http:
*/

(function ($) {
    $.fn.marquee = function (klass) {
        var newMarquee = [],
            last = this.length;

        
        
        function getReset(newDir, marqueeRedux, marqueeState) {
            var behavior = marqueeState.behavior, width = marqueeState.width, dir = marqueeState.dir;
            var r = 0;
            if (behavior == 'alternate') {
                r = newDir == 1 ? marqueeRedux[marqueeState.widthAxis] - (width*2) : width;
            } else if (behavior == 'slide') {
                if (newDir == -1) {
                    r = dir == -1 ? marqueeRedux[marqueeState.widthAxis] : width;
                } else {
                    r = dir == -1 ? marqueeRedux[marqueeState.widthAxis] - (width*2) : 0;
                }
            } else {
                r = newDir == -1 ? marqueeRedux[marqueeState.widthAxis] : 0;
            }
            return r;
        }

        
        function animateMarquee() {
            var i = newMarquee.length,
                marqueeRedux = null,
                $marqueeRedux = null,
                marqueeState = {},
                newMarqueeList = [],
                hitedge = false;
                
            while (i--) {
                marqueeRedux = newMarquee[i];
                $marqueeRedux = $(marqueeRedux);
                marqueeState = $marqueeRedux.data('marqueeState');
                
                if ($marqueeRedux.data('paused') !== true) {
                    
                    marqueeRedux[marqueeState.axis] += (marqueeState.scrollamount * marqueeState.dir);

                    
                    hitedge = marqueeState.dir == -1 ? marqueeRedux[marqueeState.axis] <= getReset(marqueeState.dir * -1, marqueeRedux, marqueeState) : marqueeRedux[marqueeState.axis] >= getReset(marqueeState.dir * -1, marqueeRedux, marqueeState);
                    
                    if ((marqueeState.behavior == 'scroll' && marqueeState.last == marqueeRedux[marqueeState.axis]) || (marqueeState.behavior == 'alternate' && hitedge && marqueeState.last != -1) || (marqueeState.behavior == 'slide' && hitedge && marqueeState.last != -1)) {                        
                        if (marqueeState.behavior == 'alternate') {
                            marqueeState.dir *= -1; 
                        }
                        marqueeState.last = -1;

                        $marqueeRedux.trigger('stop');

                        marqueeState.loops--;
                        if (marqueeState.loops === 0) {
                            if (marqueeState.behavior != 'slide') {
                                marqueeRedux[marqueeState.axis] = getReset(marqueeState.dir, marqueeRedux, marqueeState);
                            } else {
                                
                                marqueeRedux[marqueeState.axis] = getReset(marqueeState.dir * -1, marqueeRedux, marqueeState);
                            }

                            $marqueeRedux.trigger('end');
                        } else {
                            
                            newMarqueeList.push(marqueeRedux);
                            $marqueeRedux.trigger('start');
                            marqueeRedux[marqueeState.axis] = getReset(marqueeState.dir, marqueeRedux, marqueeState);
                        }
                    } else {
                        newMarqueeList.push(marqueeRedux);
                    }
                    marqueeState.last = marqueeRedux[marqueeState.axis];

                    
                    $marqueeRedux.data('marqueeState', marqueeState);
                } else {
                    
                    newMarqueeList.push(marqueeRedux);                    
                }
            }

            newMarquee = newMarqueeList;
            
            if (newMarquee.length) {
                setTimeout(animateMarquee, 25);
            }            
        }
        
        
        this.each(function (i) {
            var $marquee = $(this),
                width = $marquee.attr('width') || $marquee.width(),
                height = $marquee.attr('height') || $marquee.height(),
                wrapstyle = /left|right/.test($marquee.attr('direction')) ? 'nowrap' : 'wrap',
                $marqueeRedux = $marquee.after('<div ' + (klass ? 'class="' + klass + '" ' : '') + ' style="display: block-inline; width: ' + width + 'px; height: ' + height + 'px; overflow: hidden;"><div style="float: left; white-space: '+wrapstyle+';">' + $marquee.html() + '</div></div>').next(),
                marqueeRedux = $marqueeRedux.get(0),
                hitedge = 0,
                direction = ($marquee.attr('direction') || 'left').toLowerCase(),
                marqueeState = {
                    dir : /down|right/.test(direction) ? -1 : 1,
                    axis : /left|right/.test(direction) ? 'scrollLeft' : 'scrollTop',
                    widthAxis : /left|right/.test(direction) ? 'scrollWidth' : 'scrollHeight',
                    last : -1,
                    loops : $marquee.attr('loop') || -1,
                    scrollamount : $marquee.attr('scrollamount') || this.scrollAmount || 2,
                    behavior : ($marquee.attr('behavior') || 'scroll').toLowerCase(),
                    width : /left|right/.test(direction) ? width : height
                };
            
            
            if ($marquee.attr('loop') == -1 && marqueeState.behavior == 'slide') {
                marqueeState.loops = 1;
            }

            $marquee.remove();
            
            
            if (/left|right/.test(direction)) {
                $marqueeRedux.find('> div').css('padding', '0 ' + width + 'px');
            } else {
                $marqueeRedux.find('> div').css('padding', height + 'px 0');
                $marqueeRedux.find('> div').css('white-space', 'wrap');
                $marqueeRedux.find('> div').css('word-wrap', 'break-word');
                $marqueeRedux.find('> div').css('overflow', 'hidden');
                $marqueeRedux.find('> div').css('width', width);
            }
            
            
            $marqueeRedux.bind('stop', function () {
                $marqueeRedux.data('paused', true);
            }).bind('pause', function () {
                $marqueeRedux.data('paused', true);
            }).bind('start', function () {
                $marqueeRedux.data('paused', false);
            }).bind('unpause', function () {
                $marqueeRedux.data('paused', false);
            }).data('marqueeState', marqueeState); 
            
            

            newMarquee.push(marqueeRedux);

            marqueeRedux[marqueeState.axis] = getReset(marqueeState.dir, marqueeRedux, marqueeState);
            $marqueeRedux.trigger('start');
            
            
            if (i+1 == last) {
                animateMarquee();
            }
        });            

        return $(newMarquee);
    };
}(jQuery));
/* jFeed : jQuery feed parser plugin
 * Copyright (C) 2007 Jean-François Hovinne - http:
 * Dual licensed under the MIT (MIT-license.txt)
 * and GPL (GPL-license.txt) licenses.
 */

jQuery.getFeed = function(options) {

    options = jQuery.extend({
    
        url: null,
        data: null,
        success: null
        
    }, options);

    if(options.url) {

        $.ajax({
            type: 'GET',
            url: options.url,
            data: options.data,
            dataType: 'xml',
            async : false,
            success: function(xml) {
                var feed = new JFeed(xml);
                if(jQuery.isFunction(options.success)) options.success(feed);
            }
        });
    }
};

function JFeed(xml) {
    if(xml) this.parse(xml);
};

JFeed.prototype = {

    type: '',
    version: '',
    title: '',
    link: '',
    description: '',
    parse: function(xml) {
        
        if(jQuery('channel', xml).length == 1) {
        
            this.type = 'rss';
            var feedClass = new JRss(xml);

        } else if(jQuery('feed', xml).length == 1) {
        
            this.type = 'atom';
            var feedClass = new JAtom(xml);
        }
        
        if(feedClass) jQuery.extend(this, feedClass);
    }
};

function JFeedItem() {};

JFeedItem.prototype = {

    title: '',
    link: '',
    description: '',
    updated: '',
    id: ''
};

function JAtom(xml) {
    this._parse(xml);
};

JAtom.prototype = {
    
    _parse: function(xml) {
    
        var channel = jQuery('feed', xml).eq(0);

        this.version = '1.0';
        this.title = jQuery(channel).find('title:first').text();
        this.link = jQuery(channel).find('link:first').attr('href');
        this.description = jQuery(channel).find('subtitle:first').text();
        this.language = jQuery(channel).attr('xml:lang');
        this.updated = jQuery(channel).find('updated:first').text();
        
        this.items = new Array();
        
        var feed = this;
        
        jQuery('entry', xml).each( function() {
        
            var item = new JFeedItem();
            
            item.title = jQuery(this).find('title').eq(0).text();
            item.link = jQuery(this).find('link').eq(0).attr('href');
            item.description = jQuery(this).find('content').eq(0).text();
            item.updated = jQuery(this).find('updated').eq(0).text();
            item.id = jQuery(this).find('id').eq(0).text();
            
            feed.items.push(item);
        });
    }
};

function JRss(xml) {
    this._parse(xml);
};

JRss.prototype  = {
    
    _parse: function(xml) {
    
        if(jQuery('rss', xml).length == 0) this.version = '1.0';
        else this.version = jQuery('rss', xml).eq(0).attr('version');

        var channel = jQuery('channel', xml).eq(0);
    
        this.title = jQuery(channel).find('title:first').text();
        this.link = jQuery(channel).find('link:first').text();
        this.description = jQuery(channel).find('description:first').text();
        this.language = jQuery(channel).find('language:first').text();
        this.updated = jQuery(channel).find('lastBuildDate:first').text();
    
        this.items = new Array();
        
        var feed = this;
        
        jQuery('item', xml).each( function() {
        
            var item = new JFeedItem();
            
            item.title = jQuery(this).find('title').eq(0).text();
            item.link = jQuery(this).find('link').eq(0).text();
            item.description = jQuery(this).find('description').eq(0).text();
            item.updated = jQuery(this).find('pubDate').eq(0).text();
            item.id = jQuery(this).find('guid').eq(0).text();
            
            feed.items.push(item);
        });
    }
};


(function() {
  var Byte, Client, Frame, Stomp,
    __hasProp = {}.hasOwnProperty;

  Byte = {
    LF: '\x0A',
    NULL: '\x00'
  };

  Frame = (function() {

    function Frame(command, headers, body) {
      this.command = command;
      this.headers = headers != null ? headers : {};
      this.body = body != null ? body : '';
    }

    Frame.prototype.toString = function() {
      var lines, name, value, _ref;
      lines = [this.command];
      _ref = this.headers;
      for (name in _ref) {
        if (!__hasProp.call(_ref, name)) continue;
        value = _ref[name];
        lines.push("" + name + ":" + value);
      }
      if (this.body) {
        lines.push("content-length:" + ('' + this.body).length);
      }
      lines.push(Byte.LF + this.body);
      return lines.join(Byte.LF);
    };

    Frame._unmarshallSingle = function(data) {
      var body, chr, command, divider, headerLines, headers, i, idx, len, line, start, trim, _i, _j, _ref, _ref1;
      divider = data.search(RegExp("" + Byte.LF + Byte.LF));
      headerLines = data.substring(0, divider).split(Byte.LF);
      command = headerLines.shift();
      headers = {};
      trim = function(str) {
        return str.replace(/^\s+|\s+$/g, '');
      };
      line = idx = null;
      for (i = _i = 0, _ref = headerLines.length; 0 <= _ref ? _i < _ref : _i > _ref; i = 0 <= _ref ? ++_i : --_i) {
        line = headerLines[i];
        idx = line.indexOf(':');
        headers[trim(line.substring(0, idx))] = trim(line.substring(idx + 1));
      }
      body = '';
      start = divider + 2;
      if (headers['content-length']) {
        len = parseInt(headers['content-length']);
        body = ('' + data).substring(start, start + len);
      } else {
        chr = null;
        for (i = _j = start, _ref1 = data.length; start <= _ref1 ? _j < _ref1 : _j > _ref1; i = start <= _ref1 ? ++_j : --_j) {
          chr = data.charAt(i);
          if (chr === Byte.NULL) {
            break;
          }
          body += chr;
        }
      }
      return new Frame(command, headers, body);
    };

    Frame.unmarshall = function(datas) {
      var data;
      return (function() {
        var _i, _len, _ref, _results;
        _ref = datas.split(RegExp("" + Byte.NULL + Byte.LF + "*"));
        _results = [];
        for (_i = 0, _len = _ref.length; _i < _len; _i++) {
          data = _ref[_i];
          if ((data != null ? data.length : void 0) > 0) {
            _results.push(Frame._unmarshallSingle(data));
          }
        }
        return _results;
      })();
    };

    Frame.marshall = function(command, headers, body) {
      var frame;
      frame = new Frame(command, headers, body);
      return frame.toString() + Byte.NULL;
    };

    return Frame;

  })();

  Client = (function() {

    function Client(ws) {
      this.ws = ws;
      this.ws.binaryType = "arraybuffer";
      this.counter = 0;
      this.connected = false;
      this.heartbeat = {
        outgoing: 10000,
        incoming: 10000
      };
      this.subscriptions = {};
    }

    Client.prototype._transmit = function(command, headers, body) {
      var out;
      out = Frame.marshall(command, headers, body);
      if (typeof this.debug === "function") {
        this.debug(">>> " + out);
      }
      return this.ws.send(out);
    };

    Client.prototype._setupHeartbeat = function(headers) {
      var serverIncoming, serverOutgoing, ttl, v, _ref, _ref1,
        _this = this;
      if ((_ref = headers.version) !== Stomp.VERSIONS.V1_1 && _ref !== Stomp.VERSIONS.V1_2) {
        return;
      }
      _ref1 = (function() {
        var _i, _len, _ref1, _results;
        _ref1 = headers['heart-beat'].split(",");
        _results = [];
        for (_i = 0, _len = _ref1.length; _i < _len; _i++) {
          v = _ref1[_i];
          _results.push(parseInt(v));
        }
        return _results;
      })(), serverOutgoing = _ref1[0], serverIncoming = _ref1[1];
      if (!(this.heartbeat.outgoing === 0 || serverIncoming === 0)) {
        ttl = Math.max(this.heartbeat.outgoing, serverIncoming);
        if (typeof this.debug === "function") {
          this.debug("send PING every " + ttl + "ms");
        }
        this.pinger = typeof window !== "undefined" && window !== null ? window.setInterval(function() {
          _this.ws.send(Byte.LF);
          return typeof _this.debug === "function" ? _this.debug(">>> PING") : void 0;
        }, ttl) : void 0;
      }
      if (!(this.heartbeat.incoming === 0 || serverOutgoing === 0)) {
        ttl = Math.max(this.heartbeat.incoming, serverOutgoing);
        if (typeof this.debug === "function") {
          this.debug("check PONG every " + ttl + "ms");
        }
        return this.ponger = typeof window !== "undefined" && window !== null ? window.setInterval(function() {
          var delta;
          delta = Date.now() - _this.serverActivity;
          if (delta > ttl * 2) {
            if (typeof _this.debug === "function") {
              _this.debug("did not receive server activity for the last " + delta + "ms");
            }
            return _this._cleanUp();
          }
        }, ttl) : void 0;
      }
    };

    Client.prototype.connect = function(login, passcode, connectCallback, errorCallback, vhost) {
      var _this = this;
      this.connectCallback = connectCallback;
      if (typeof this.debug === "function") {
        this.debug("Opening Web Socket...");
      }
      this.ws.onmessage = function(evt) {
        var arr, c, data, frame, onreceive, _i, _len, _ref, _results;
        data = typeof ArrayBuffer !== 'undefined' && evt.data instanceof ArrayBuffer ? (arr = new Uint8Array(evt.data), typeof _this.debug === "function" ? _this.debug("--- got data length: " + arr.length) : void 0, ((function() {
          var _i, _len, _results;
          _results = [];
          for (_i = 0, _len = arr.length; _i < _len; _i++) {
            c = arr[_i];
            _results.push(String.fromCharCode(c));
          }
          return _results;
        })()).join('')) : evt.data;
        _this.serverActivity = Date.now();
        if (data === Byte.LF) {
          if (typeof _this.debug === "function") {
            _this.debug("<<< PONG");
          }
          return;
        }
        if (typeof _this.debug === "function") {
          _this.debug("<<< " + data);
        }
        _ref = Frame.unmarshall(data);
        _results = [];
        for (_i = 0, _len = _ref.length; _i < _len; _i++) {
          frame = _ref[_i];
          switch (frame.command) {
            case "CONNECTED":
              if (typeof _this.debug === "function") {
                _this.debug("connected to server " + frame.headers.server);
              }
              _this.connected = true;
              _this._setupHeartbeat(frame.headers);
              _results.push(typeof _this.connectCallback === "function" ? _this.connectCallback(frame) : void 0);
              break;
            case "MESSAGE":
              onreceive = _this.subscriptions[frame.headers.subscription];
              _results.push(typeof onreceive === "function" ? onreceive(frame) : void 0);
              break;
            case "RECEIPT":
              _results.push(typeof _this.onreceipt === "function" ? _this.onreceipt(frame) : void 0);
              break;
            case "ERROR":
              _results.push(typeof errorCallback === "function" ? errorCallback(frame) : void 0);
              break;
            default:
              _results.push(typeof _this.debug === "function" ? _this.debug("Unhandled frame: " + frame) : void 0);
          }
        }
        return _results;
      };
      this.ws.onclose = function() {
        var msg;
        msg = "Whoops! Lost connection to " + _this.ws.url;
        if (typeof _this.debug === "function") {
          _this.debug(msg);
        }
        return typeof errorCallback === "function" ? errorCallback(msg) : void 0;
      };
      return this.ws.onopen = function() {
        var headers;
        if (typeof _this.debug === "function") {
          _this.debug('Web Socket Opened...');
        }
        headers = {
          "accept-version": Stomp.VERSIONS.supportedVersions(),
          "heart-beat": [_this.heartbeat.outgoing, _this.heartbeat.incoming].join(',')
        };
        if (vhost) {
          headers.host = vhost;
        }
        if (login) {
          headers.login = login;
        }
        if (passcode) {
          headers.passcode = passcode;
        }
        return _this._transmit("CONNECT", headers);
      };
    };

    Client.prototype.disconnect = function(disconnectCallback) {
      this._transmit("DISCONNECT");
      this.ws.onclose = null;
      this._cleanUp();
      return typeof disconnectCallback === "function" ? disconnectCallback() : void 0;
    };

    Client.prototype._cleanUp = function() {
      this.ws.close();
      this.connected = false;
      if (this.pinger) {
        if (typeof window !== "undefined" && window !== null) {
          window.clearInterval(this.pinger);
        }
      }
      if (this.ponger) {
        return typeof window !== "undefined" && window !== null ? window.clearInterval(this.ponger) : void 0;
      }
    };

    Client.prototype.send = function(destination, headers, body) {
      if (headers == null) {
        headers = {};
      }
      if (body == null) {
        body = '';
      }
      headers.destination = destination;
      return this._transmit("SEND", headers, body);
    };

    Client.prototype.subscribe = function(destination, callback, headers) {
      if (headers == null) {
        headers = {};
      }
      if (!headers.id) {
        headers.id = "sub-" + this.counter++;
      }
      headers.destination = destination;
      this.subscriptions[headers.id] = callback;
      this._transmit("SUBSCRIBE", headers);
      return headers.id;
    };

    Client.prototype.unsubscribe = function(id) {
      delete this.subscriptions[id];
      return this._transmit("UNSUBSCRIBE", {
        id: id
      });
    };

    Client.prototype.begin = function(transaction) {
      return this._transmit("BEGIN", {
        transaction: transaction
      });
    };

    Client.prototype.commit = function(transaction) {
      return this._transmit("COMMIT", {
        transaction: transaction
      });
    };

    Client.prototype.abort = function(transaction) {
      return this._transmit("ABORT", {
        transaction: transaction
      });
    };

    Client.prototype.ack = function(messageID, subscription, headers) {
      if (headers == null) {
        headers = {};
      }
      headers["message-id"] = messageID;
      headers.subscription = subscription;
      return this._transmit("ACK", headers);
    };

    Client.prototype.nack = function(messageID, subscription, headers) {
      if (headers == null) {
        headers = {};
      }
      headers["message-id"] = messageID;
      headers.subscription = subscription;
      return this._transmit("NACK", headers);
    };

    return Client;

  })();

  Stomp = {
    libVersion: "2.0.0-next",
    VERSIONS: {
      V1_0: '1.0',
      V1_1: '1.1',
      V1_2: '1.2',
      supportedVersions: function() {
        return '1.1,1.0';
      }
    },
    client: function(url, protocols) {
      var klass, ws;
      if (protocols == null) {
        protocols = ['v10.stomp', 'v11.stomp'];
      }
      klass = Stomp.WebSocketClass || WebSocket;
      ws = new klass(url, protocols);
      return new Client(ws);
    },
    over: function(ws) {
      return new Client(ws);
    },
    Frame: Frame
  };

  if (typeof window !== "undefined" && window !== null) {
    window.Stomp = Stomp;
  } else {
    exports.Stomp = Stomp;
    Stomp.WebSocketClass = require('./test/server.mock.js').StompServerMock;
  }

}).call(this);
/*
 * jPlayer Plugin for jQuery JavaScript Library
 * http:
 *
 * Copyright (c) 2009 - 2012 Happyworm Ltd
 * Dual licensed under the MIT and GPL licenses.
 *  - http:
 *  - http:
 *
 * Author: Mark J Panaghiston
 * Version: 2.2.0
 * Date: 13th September 2012
 */

(function(b,f){b.fn.jPlayer=function(a){var c="string"===typeof a,d=Array.prototype.slice.call(arguments,1),e=this,a=!c&&d.length?b.extend.apply(null,[!0,a].concat(d)):a;if(c&&"_"===a.charAt(0))return e;c?this.each(function(){var c=b.data(this,"jPlayer"),h=c&&b.isFunction(c[a])?c[a].apply(c,d):c;if(h!==c&&h!==f)return e=h,!1}):this.each(function(){var c=b.data(this,"jPlayer");c?c.option(a||{}):b.data(this,"jPlayer",new b.jPlayer(a,this))});return e};b.jPlayer=function(a,c){if(arguments.length){this.element=
b(c);this.options=b.extend(!0,{},this.options,a);var d=this;this.element.bind("remove.jPlayer",function(){d.destroy()});this._init()}};b.jPlayer.emulateMethods="load play pause";b.jPlayer.emulateStatus="src readyState networkState currentTime duration paused ended playbackRate";b.jPlayer.emulateOptions="muted volume";b.jPlayer.reservedEvent="ready flashreset resize repeat error warning";b.jPlayer.event={ready:"jPlayer_ready",flashreset:"jPlayer_flashreset",resize:"jPlayer_resize",repeat:"jPlayer_repeat",
click:"jPlayer_click",error:"jPlayer_error",warning:"jPlayer_warning",loadstart:"jPlayer_loadstart",progress:"jPlayer_progress",suspend:"jPlayer_suspend",abort:"jPlayer_abort",emptied:"jPlayer_emptied",stalled:"jPlayer_stalled",play:"jPlayer_play",pause:"jPlayer_pause",loadedmetadata:"jPlayer_loadedmetadata",loadeddata:"jPlayer_loadeddata",waiting:"jPlayer_waiting",playing:"jPlayer_playing",canplay:"jPlayer_canplay",canplaythrough:"jPlayer_canplaythrough",seeking:"jPlayer_seeking",seeked:"jPlayer_seeked",
timeupdate:"jPlayer_timeupdate",ended:"jPlayer_ended",ratechange:"jPlayer_ratechange",durationchange:"jPlayer_durationchange",volumechange:"jPlayer_volumechange"};b.jPlayer.htmlEvent="loadstart abort emptied stalled loadedmetadata loadeddata canplay canplaythrough ratechange".split(" ");b.jPlayer.pause=function(){b.each(b.jPlayer.prototype.instances,function(a,c){c.data("jPlayer").status.srcSet&&c.jPlayer("pause")})};b.jPlayer.timeFormat={showHour:!1,showMin:!0,showSec:!0,padHour:!1,padMin:!0,padSec:!0,
sepHour:":",sepMin:":",sepSec:""};b.jPlayer.convertTime=function(a){var c=new Date(1E3*a),d=c.getUTCHours(),a=c.getUTCMinutes(),c=c.getUTCSeconds(),d=b.jPlayer.timeFormat.padHour&&10>d?"0"+d:d,a=b.jPlayer.timeFormat.padMin&&10>a?"0"+a:a,c=b.jPlayer.timeFormat.padSec&&10>c?"0"+c:c;return(b.jPlayer.timeFormat.showHour?d+b.jPlayer.timeFormat.sepHour:"")+(b.jPlayer.timeFormat.showMin?a+b.jPlayer.timeFormat.sepMin:"")+(b.jPlayer.timeFormat.showSec?c+b.jPlayer.timeFormat.sepSec:"")};b.jPlayer.uaBrowser=
function(a){var a=a.toLowerCase(),c=/(opera)(?:.*version)?[ \/]([\w.]+)/,b=/(msie) ([\w.]+)/,e=/(mozilla)(?:.*? rv:([\w.]+))?/,a=/(webkit)[ \/]([\w.]+)/.exec(a)||c.exec(a)||b.exec(a)||0>a.indexOf("compatible")&&e.exec(a)||[];return{browser:a[1]||"",version:a[2]||"0"}};b.jPlayer.uaPlatform=function(a){var b=a.toLowerCase(),d=/(android)/,e=/(mobile)/,a=/(ipad|iphone|ipod|android|blackberry|playbook|windows ce|webos)/.exec(b)||[],b=/(ipad|playbook)/.exec(b)||!e.exec(b)&&d.exec(b)||[];a[1]&&(a[1]=a[1].replace(/\s/g,
"_"));return{platform:a[1]||"",tablet:b[1]||""}};b.jPlayer.browser={};b.jPlayer.platform={};var i=b.jPlayer.uaBrowser(navigator.userAgent);i.browser&&(b.jPlayer.browser[i.browser]=!0,b.jPlayer.browser.version=i.version);i=b.jPlayer.uaPlatform(navigator.userAgent);i.platform&&(b.jPlayer.platform[i.platform]=!0,b.jPlayer.platform.mobile=!i.tablet,b.jPlayer.platform.tablet=!!i.tablet);b.jPlayer.prototype={count:0,version:{script:"2.2.0",needFlash:"2.2.0",flash:"unknown"},options:{swfPath:"js",solution:"html, flash",
supplied:"mp3",preload:"metadata",volume:0.8,muted:!1,wmode:"opaque",backgroundColor:"#000000",cssSelectorAncestor:"#jp_container_1",cssSelector:{videoPlay:".jp-video-play",play:".jp-play",pause:".jp-pause",stop:".jp-stop",seekBar:".jp-seek-bar",playBar:".jp-play-bar",mute:".jp-mute",unmute:".jp-unmute",volumeBar:".jp-volume-bar",volumeBarValue:".jp-volume-bar-value",volumeMax:".jp-volume-max",currentTime:".jp-current-time",duration:".jp-duration",fullScreen:".jp-full-screen",restoreScreen:".jp-restore-screen",
repeat:".jp-repeat",repeatOff:".jp-repeat-off",gui:".jp-gui",noSolution:".jp-no-solution"},fullScreen:!1,autohide:{restored:!1,full:!0,fadeIn:200,fadeOut:600,hold:1E3},loop:!1,repeat:function(a){a.jPlayer.options.loop?b(this).unbind(".jPlayerRepeat").bind(b.jPlayer.event.ended+".jPlayer.jPlayerRepeat",function(){b(this).jPlayer("play")}):b(this).unbind(".jPlayerRepeat")},nativeVideoControls:{},noFullScreen:{msie:/msie [0-6]/,ipad:/ipad.*?os [0-4]/,iphone:/iphone/,ipod:/ipod/,android_pad:/android [0-3](?!.*?mobile)/,
android_phone:/android.*?mobile/,blackberry:/blackberry/,windows_ce:/windows ce/,webos:/webos/},noVolume:{ipad:/ipad/,iphone:/iphone/,ipod:/ipod/,android_pad:/android(?!.*?mobile)/,android_phone:/android.*?mobile/,blackberry:/blackberry/,windows_ce:/windows ce/,webos:/webos/,playbook:/playbook/},verticalVolume:!1,idPrefix:"jp",noConflict:"jQuery",emulateHtml:!1,errorAlerts:!1,warningAlerts:!1},optionsAudio:{size:{width:"0px",height:"0px",cssClass:""},sizeFull:{width:"0px",height:"0px",cssClass:""}},
optionsVideo:{size:{width:"480px",height:"270px",cssClass:"jp-video-270p"},sizeFull:{width:"100%",height:"100%",cssClass:"jp-video-full"}},instances:{},status:{src:"",media:{},paused:!0,format:{},formatType:"",waitForPlay:!0,waitForLoad:!0,srcSet:!1,video:!1,seekPercent:0,currentPercentRelative:0,currentPercentAbsolute:0,currentTime:0,duration:0,readyState:0,networkState:0,playbackRate:1,ended:0},internal:{ready:!1},solution:{html:!0,flash:!0},format:{mp3:{codec:'audio/mpeg; codecs="mp3"',flashCanPlay:!0,
media:"audio"},m4a:{codec:'audio/mp4; codecs="mp4a.40.2"',flashCanPlay:!0,media:"audio"},oga:{codec:'audio/ogg; codecs="vorbis"',flashCanPlay:!1,media:"audio"},wav:{codec:'audio/wav; codecs="1"',flashCanPlay:!1,media:"audio"},webma:{codec:'audio/webm; codecs="vorbis"',flashCanPlay:!1,media:"audio"},fla:{codec:"audio/x-flv",flashCanPlay:!0,media:"audio"},rtmpa:{codec:'audio/rtmp; codecs="rtmp"',flashCanPlay:!0,media:"audio"},m4v:{codec:'video/mp4; codecs="avc1.42E01E, mp4a.40.2"',flashCanPlay:!0,media:"video"},
ogv:{codec:'video/ogg; codecs="theora, vorbis"',flashCanPlay:!1,media:"video"},webmv:{codec:'video/webm; codecs="vorbis, vp8"',flashCanPlay:!1,media:"video"},flv:{codec:"video/x-flv",flashCanPlay:!0,media:"video"},rtmpv:{codec:'video/rtmp; codecs="rtmp"',flashCanPlay:!0,media:"video"}},_init:function(){var a=this;this.element.empty();this.status=b.extend({},this.status);this.internal=b.extend({},this.internal);this.internal.domNode=this.element.get(0);this.formats=[];this.solutions=[];this.require=
{};this.htmlElement={};this.html={};this.html.audio={};this.html.video={};this.flash={};this.css={};this.css.cs={};this.css.jq={};this.ancestorJq=[];this.options.volume=this._limitValue(this.options.volume,0,1);b.each(this.options.supplied.toLowerCase().split(","),function(c,d){var e=d.replace(/^\s+|\s+$/g,"");if(a.format[e]){var f=false;b.each(a.formats,function(a,b){if(e===b){f=true;return false}});f||a.formats.push(e)}});b.each(this.options.solution.toLowerCase().split(","),function(c,d){var e=
d.replace(/^\s+|\s+$/g,"");if(a.solution[e]){var f=false;b.each(a.solutions,function(a,b){if(e===b){f=true;return false}});f||a.solutions.push(e)}});this.internal.instance="jp_"+this.count;this.instances[this.internal.instance]=this.element;this.element.attr("id")||this.element.attr("id",this.options.idPrefix+"_jplayer_"+this.count);this.internal.self=b.extend({},{id:this.element.attr("id"),jq:this.element});this.internal.audio=b.extend({},{id:this.options.idPrefix+"_audio_"+this.count,jq:f});this.internal.video=
b.extend({},{id:this.options.idPrefix+"_video_"+this.count,jq:f});this.internal.flash=b.extend({},{id:this.options.idPrefix+"_flash_"+this.count,jq:f,swf:this.options.swfPath+(this.options.swfPath.toLowerCase().slice(-4)!==".swf"?(this.options.swfPath&&this.options.swfPath.slice(-1)!=="/"?"/":"")+"Jplayer.swf":"")});this.internal.poster=b.extend({},{id:this.options.idPrefix+"_poster_"+this.count,jq:f});b.each(b.jPlayer.event,function(b,c){if(a.options[b]!==f){a.element.bind(c+".jPlayer",a.options[b]);
a.options[b]=f}});this.require.audio=false;this.require.video=false;b.each(this.formats,function(b,c){a.require[a.format[c].media]=true});this.options=this.require.video?b.extend(true,{},this.optionsVideo,this.options):b.extend(true,{},this.optionsAudio,this.options);this._setSize();this.status.nativeVideoControls=this._uaBlocklist(this.options.nativeVideoControls);this.status.noFullScreen=this._uaBlocklist(this.options.noFullScreen);this.status.noVolume=this._uaBlocklist(this.options.noVolume);this._restrictNativeVideoControls();
this.htmlElement.poster=document.createElement("img");this.htmlElement.poster.id=this.internal.poster.id;this.htmlElement.poster.onload=function(){(!a.status.video||a.status.waitForPlay)&&a.internal.poster.jq.show()};this.element.append(this.htmlElement.poster);this.internal.poster.jq=b("#"+this.internal.poster.id);this.internal.poster.jq.css({width:this.status.width,height:this.status.height});this.internal.poster.jq.hide();this.internal.poster.jq.bind("click.jPlayer",function(){a._trigger(b.jPlayer.event.click)});
this.html.audio.available=false;if(this.require.audio){this.htmlElement.audio=document.createElement("audio");this.htmlElement.audio.id=this.internal.audio.id;this.html.audio.available=!!this.htmlElement.audio.canPlayType&&this._testCanPlayType(this.htmlElement.audio)}this.html.video.available=false;if(this.require.video){this.htmlElement.video=document.createElement("video");this.htmlElement.video.id=this.internal.video.id;this.html.video.available=!!this.htmlElement.video.canPlayType&&this._testCanPlayType(this.htmlElement.video)}this.flash.available=
this._checkForFlash(10);this.html.canPlay={};this.flash.canPlay={};b.each(this.formats,function(b,c){a.html.canPlay[c]=a.html[a.format[c].media].available&&""!==a.htmlElement[a.format[c].media].canPlayType(a.format[c].codec);a.flash.canPlay[c]=a.format[c].flashCanPlay&&a.flash.available});this.html.desired=false;this.flash.desired=false;b.each(this.solutions,function(c,d){if(c===0)a[d].desired=true;else{var e=false,f=false;b.each(a.formats,function(b,c){a[a.solutions[0]].canPlay[c]&&(a.format[c].media===
"video"?f=true:e=true)});a[d].desired=a.require.audio&&!e||a.require.video&&!f}});this.html.support={};this.flash.support={};b.each(this.formats,function(b,c){a.html.support[c]=a.html.canPlay[c]&&a.html.desired;a.flash.support[c]=a.flash.canPlay[c]&&a.flash.desired});this.html.used=false;this.flash.used=false;b.each(this.solutions,function(c,d){b.each(a.formats,function(b,c){if(a[d].support[c]){a[d].used=true;return false}})});this._resetActive();this._resetGate();this._cssSelectorAncestor(this.options.cssSelectorAncestor);
if(!this.html.used&&!this.flash.used){this._error({type:b.jPlayer.error.NO_SOLUTION,context:"{solution:'"+this.options.solution+"', supplied:'"+this.options.supplied+"'}",message:b.jPlayer.errorMsg.NO_SOLUTION,hint:b.jPlayer.errorHint.NO_SOLUTION});this.css.jq.noSolution.length&&this.css.jq.noSolution.show()}else this.css.jq.noSolution.length&&this.css.jq.noSolution.hide();if(this.flash.used){var c,d="jQuery="+encodeURI(this.options.noConflict)+"&id="+encodeURI(this.internal.self.id)+"&vol="+this.options.volume+
"&muted="+this.options.muted;if(b.jPlayer.browser.msie&&Number(b.jPlayer.browser.version)<=8){d=['<param name="movie" value="'+this.internal.flash.swf+'" />','<param name="FlashVars" value="'+d+'" />','<param name="allowScriptAccess" value="always" />','<param name="bgcolor" value="'+this.options.backgroundColor+'" />','<param name="wmode" value="'+this.options.wmode+'" />'];c=document.createElement('<object id="'+this.internal.flash.id+'" classid="clsid:d27cdb6e-ae6d-11cf-96b8-444553540000" width="0" height="0"></object>');
for(var e=0;e<d.length;e++)c.appendChild(document.createElement(d[e]))}else{e=function(a,b,c){var d=document.createElement("param");d.setAttribute("name",b);d.setAttribute("value",c);a.appendChild(d)};c=document.createElement("object");c.setAttribute("id",this.internal.flash.id);c.setAttribute("data",this.internal.flash.swf);c.setAttribute("type","application/x-shockwave-flash");c.setAttribute("width","1");c.setAttribute("height","1");e(c,"flashvars",d);e(c,"allowscriptaccess","always");e(c,"bgcolor",
this.options.backgroundColor);e(c,"wmode",this.options.wmode)}this.element.append(c);this.internal.flash.jq=b(c)}if(this.html.used){if(this.html.audio.available){this._addHtmlEventListeners(this.htmlElement.audio,this.html.audio);this.element.append(this.htmlElement.audio);this.internal.audio.jq=b("#"+this.internal.audio.id)}if(this.html.video.available){this._addHtmlEventListeners(this.htmlElement.video,this.html.video);this.element.append(this.htmlElement.video);this.internal.video.jq=b("#"+this.internal.video.id);
this.status.nativeVideoControls?this.internal.video.jq.css({width:this.status.width,height:this.status.height}):this.internal.video.jq.css({width:"0px",height:"0px"});this.internal.video.jq.bind("click.jPlayer",function(){a._trigger(b.jPlayer.event.click)})}}this.options.emulateHtml&&this._emulateHtmlBridge();this.html.used&&!this.flash.used&&setTimeout(function(){a.internal.ready=true;a.version.flash="n/a";a._trigger(b.jPlayer.event.repeat);a._trigger(b.jPlayer.event.ready)},100);this._updateNativeVideoControls();
this._updateInterface();this._updateButtons(false);this._updateAutohide();this._updateVolume(this.options.volume);this._updateMute(this.options.muted);this.css.jq.videoPlay.length&&this.css.jq.videoPlay.hide();b.jPlayer.prototype.count++},destroy:function(){this.clearMedia();this._removeUiClass();this.css.jq.currentTime.length&&this.css.jq.currentTime.text("");this.css.jq.duration.length&&this.css.jq.duration.text("");b.each(this.css.jq,function(a,b){b.length&&b.unbind(".jPlayer")});this.internal.poster.jq.unbind(".jPlayer");
this.internal.video.jq&&this.internal.video.jq.unbind(".jPlayer");this.options.emulateHtml&&this._destroyHtmlBridge();this.element.removeData("jPlayer");this.element.unbind(".jPlayer");this.element.empty();delete this.instances[this.internal.instance]},enable:function(){},disable:function(){},_testCanPlayType:function(a){try{a.canPlayType(this.format.mp3.codec);return true}catch(b){return false}},_uaBlocklist:function(a){var c=navigator.userAgent.toLowerCase(),d=false;b.each(a,function(a,b){if(b&&
b.test(c)){d=true;return false}});return d},_restrictNativeVideoControls:function(){if(this.require.audio&&this.status.nativeVideoControls){this.status.nativeVideoControls=false;this.status.noFullScreen=true}},_updateNativeVideoControls:function(){if(this.html.video.available&&this.html.used){this.htmlElement.video.controls=this.status.nativeVideoControls;this._updateAutohide();if(this.status.nativeVideoControls&&this.require.video){this.internal.poster.jq.hide();this.internal.video.jq.css({width:this.status.width,
height:this.status.height})}else if(this.status.waitForPlay&&this.status.video){this.internal.poster.jq.show();this.internal.video.jq.css({width:"0px",height:"0px"})}}},_addHtmlEventListeners:function(a,c){var d=this;a.preload=this.options.preload;a.muted=this.options.muted;a.volume=this.options.volume;a.addEventListener("progress",function(){if(c.gate){d._getHtmlStatus(a);d._updateInterface();d._trigger(b.jPlayer.event.progress)}},false);a.addEventListener("timeupdate",function(){if(c.gate){d._getHtmlStatus(a);
d._updateInterface();d._trigger(b.jPlayer.event.timeupdate)}},false);a.addEventListener("durationchange",function(){if(c.gate){d._getHtmlStatus(a);d._updateInterface();d._trigger(b.jPlayer.event.durationchange)}},false);a.addEventListener("play",function(){if(c.gate){d._updateButtons(true);d._html_checkWaitForPlay();d._trigger(b.jPlayer.event.play)}},false);a.addEventListener("playing",function(){if(c.gate){d._updateButtons(true);d._seeked();d._trigger(b.jPlayer.event.playing)}},false);a.addEventListener("pause",
function(){if(c.gate){d._updateButtons(false);d._trigger(b.jPlayer.event.pause)}},false);a.addEventListener("waiting",function(){if(c.gate){d._seeking();d._trigger(b.jPlayer.event.waiting)}},false);a.addEventListener("seeking",function(){if(c.gate){d._seeking();d._trigger(b.jPlayer.event.seeking)}},false);a.addEventListener("seeked",function(){if(c.gate){d._seeked();d._trigger(b.jPlayer.event.seeked)}},false);a.addEventListener("volumechange",function(){if(c.gate){d.options.volume=a.volume;d.options.muted=
a.muted;d._updateMute();d._updateVolume();d._trigger(b.jPlayer.event.volumechange)}},false);a.addEventListener("suspend",function(){if(c.gate){d._seeked();d._trigger(b.jPlayer.event.suspend)}},false);a.addEventListener("ended",function(){if(c.gate){if(!b.jPlayer.browser.webkit)d.htmlElement.media.currentTime=0;d.htmlElement.media.pause();d._updateButtons(false);d._getHtmlStatus(a,true);d._updateInterface();d._trigger(b.jPlayer.event.ended)}},false);a.addEventListener("error",function(){if(c.gate){d._updateButtons(false);
d._seeked();if(d.status.srcSet){clearTimeout(d.internal.htmlDlyCmdId);d.status.waitForLoad=true;d.status.waitForPlay=true;d.status.video&&!d.status.nativeVideoControls&&d.internal.video.jq.css({width:"0px",height:"0px"});d._validString(d.status.media.poster)&&!d.status.nativeVideoControls&&d.internal.poster.jq.show();d.css.jq.videoPlay.length&&d.css.jq.videoPlay.show();d._error({type:b.jPlayer.error.URL,context:d.status.src,message:b.jPlayer.errorMsg.URL,hint:b.jPlayer.errorHint.URL})}}},false);b.each(b.jPlayer.htmlEvent,
function(e,g){a.addEventListener(this,function(){c.gate&&d._trigger(b.jPlayer.event[g])},false)})},_getHtmlStatus:function(a,b){var d=0,e=0,g=0,f=0;if(isFinite(a.duration))this.status.duration=a.duration;d=a.currentTime;e=this.status.duration>0?100*d/this.status.duration:0;if(typeof a.seekable==="object"&&a.seekable.length>0){g=this.status.duration>0?100*a.seekable.end(a.seekable.length-1)/this.status.duration:100;f=this.status.duration>0?100*a.currentTime/a.seekable.end(a.seekable.length-1):0}else{g=
100;f=e}if(b)e=f=d=0;this.status.seekPercent=g;this.status.currentPercentRelative=f;this.status.currentPercentAbsolute=e;this.status.currentTime=d;this.status.readyState=a.readyState;this.status.networkState=a.networkState;this.status.playbackRate=a.playbackRate;this.status.ended=a.ended},_resetStatus:function(){this.status=b.extend({},this.status,b.jPlayer.prototype.status)},_trigger:function(a,c,d){a=b.Event(a);a.jPlayer={};a.jPlayer.version=b.extend({},this.version);a.jPlayer.options=b.extend(true,
{},this.options);a.jPlayer.status=b.extend(true,{},this.status);a.jPlayer.html=b.extend(true,{},this.html);a.jPlayer.flash=b.extend(true,{},this.flash);if(c)a.jPlayer.error=b.extend({},c);if(d)a.jPlayer.warning=b.extend({},d);this.element.trigger(a)},jPlayerFlashEvent:function(a,c){if(a===b.jPlayer.event.ready)if(this.internal.ready){if(this.flash.gate){if(this.status.srcSet){var d=this.status.currentTime,e=this.status.paused;this.setMedia(this.status.media);d>0&&(e?this.pause(d):this.play(d))}this._trigger(b.jPlayer.event.flashreset)}}else{this.internal.ready=
true;this.internal.flash.jq.css({width:"0px",height:"0px"});this.version.flash=c.version;this.version.needFlash!==this.version.flash&&this._error({type:b.jPlayer.error.VERSION,context:this.version.flash,message:b.jPlayer.errorMsg.VERSION+this.version.flash,hint:b.jPlayer.errorHint.VERSION});this._trigger(b.jPlayer.event.repeat);this._trigger(a)}if(this.flash.gate)switch(a){case b.jPlayer.event.progress:this._getFlashStatus(c);this._updateInterface();this._trigger(a);break;case b.jPlayer.event.timeupdate:this._getFlashStatus(c);
this._updateInterface();this._trigger(a);break;case b.jPlayer.event.play:this._seeked();this._updateButtons(true);this._trigger(a);break;case b.jPlayer.event.pause:this._updateButtons(false);this._trigger(a);break;case b.jPlayer.event.ended:this._updateButtons(false);this._trigger(a);break;case b.jPlayer.event.click:this._trigger(a);break;case b.jPlayer.event.error:this.status.waitForLoad=true;this.status.waitForPlay=true;this.status.video&&this.internal.flash.jq.css({width:"0px",height:"0px"});this._validString(this.status.media.poster)&&
this.internal.poster.jq.show();this.css.jq.videoPlay.length&&this.status.video&&this.css.jq.videoPlay.show();this.status.video?this._flash_setVideo(this.status.media):this._flash_setAudio(this.status.media);this._updateButtons(false);this._error({type:b.jPlayer.error.URL,context:c.src,message:b.jPlayer.errorMsg.URL,hint:b.jPlayer.errorHint.URL});break;case b.jPlayer.event.seeking:this._seeking();this._trigger(a);break;case b.jPlayer.event.seeked:this._seeked();this._trigger(a);break;case b.jPlayer.event.ready:break;
default:this._trigger(a)}return false},_getFlashStatus:function(a){this.status.seekPercent=a.seekPercent;this.status.currentPercentRelative=a.currentPercentRelative;this.status.currentPercentAbsolute=a.currentPercentAbsolute;this.status.currentTime=a.currentTime;this.status.duration=a.duration;this.status.readyState=4;this.status.networkState=0;this.status.playbackRate=1;this.status.ended=false},_updateButtons:function(a){if(a!==f){this.status.paused=!a;if(this.css.jq.play.length&&this.css.jq.pause.length)if(a){this.css.jq.play.hide();
this.css.jq.pause.show()}else{this.css.jq.play.show();this.css.jq.pause.hide()}}if(this.css.jq.restoreScreen.length&&this.css.jq.fullScreen.length)if(this.status.noFullScreen){this.css.jq.fullScreen.hide();this.css.jq.restoreScreen.hide()}else if(this.options.fullScreen){this.css.jq.fullScreen.hide();this.css.jq.restoreScreen.show()}else{this.css.jq.fullScreen.show();this.css.jq.restoreScreen.hide()}if(this.css.jq.repeat.length&&this.css.jq.repeatOff.length)if(this.options.loop){this.css.jq.repeat.hide();
this.css.jq.repeatOff.show()}else{this.css.jq.repeat.show();this.css.jq.repeatOff.hide()}},_updateInterface:function(){this.css.jq.seekBar.length&&this.css.jq.seekBar.width(this.status.seekPercent+"%");this.css.jq.playBar.length&&this.css.jq.playBar.width(this.status.currentPercentRelative+"%");this.css.jq.currentTime.length&&this.css.jq.currentTime.text(b.jPlayer.convertTime(this.status.currentTime));this.css.jq.duration.length&&this.css.jq.duration.text(b.jPlayer.convertTime(this.status.duration))},
_seeking:function(){this.css.jq.seekBar.length&&this.css.jq.seekBar.addClass("jp-seeking-bg")},_seeked:function(){this.css.jq.seekBar.length&&this.css.jq.seekBar.removeClass("jp-seeking-bg")},_resetGate:function(){this.html.audio.gate=false;this.html.video.gate=false;this.flash.gate=false},_resetActive:function(){this.html.active=false;this.flash.active=false},setMedia:function(a){var c=this,d=false,e=this.status.media.poster!==a.poster;this._resetMedia();this._resetGate();this._resetActive();b.each(this.formats,
function(e,f){var i=c.format[f].media==="video";b.each(c.solutions,function(b,e){if(c[e].support[f]&&c._validString(a[f])){var g=e==="html";if(i){if(g){c.html.video.gate=true;c._html_setVideo(a);c.html.active=true}else{c.flash.gate=true;c._flash_setVideo(a);c.flash.active=true}c.css.jq.videoPlay.length&&c.css.jq.videoPlay.show();c.status.video=true}else{if(g){c.html.audio.gate=true;c._html_setAudio(a);c.html.active=true}else{c.flash.gate=true;c._flash_setAudio(a);c.flash.active=true}c.css.jq.videoPlay.length&&
c.css.jq.videoPlay.hide();c.status.video=false}d=true;return false}});if(d)return false});if(d){if((!this.status.nativeVideoControls||!this.html.video.gate)&&this._validString(a.poster))e?this.htmlElement.poster.src=a.poster:this.internal.poster.jq.show();this.status.srcSet=true;this.status.media=b.extend({},a);this._updateButtons(false);this._updateInterface()}else this._error({type:b.jPlayer.error.NO_SUPPORT,context:"{supplied:'"+this.options.supplied+"'}",message:b.jPlayer.errorMsg.NO_SUPPORT,
hint:b.jPlayer.errorHint.NO_SUPPORT})},_resetMedia:function(){this._resetStatus();this._updateButtons(false);this._updateInterface();this._seeked();this.internal.poster.jq.hide();clearTimeout(this.internal.htmlDlyCmdId);this.html.active?this._html_resetMedia():this.flash.active&&this._flash_resetMedia()},clearMedia:function(){this._resetMedia();this.html.active?this._html_clearMedia():this.flash.active&&this._flash_clearMedia();this._resetGate();this._resetActive()},load:function(){this.status.srcSet?
this.html.active?this._html_load():this.flash.active&&this._flash_load():this._urlNotSetError("load")},play:function(a){a=typeof a==="number"?a:NaN;this.status.srcSet?this.html.active?this._html_play(a):this.flash.active&&this._flash_play(a):this._urlNotSetError("play")},videoPlay:function(){this.play()},pause:function(a){a=typeof a==="number"?a:NaN;this.status.srcSet?this.html.active?this._html_pause(a):this.flash.active&&this._flash_pause(a):this._urlNotSetError("pause")},pauseOthers:function(){var a=
this;b.each(this.instances,function(b,d){a.element!==d&&d.data("jPlayer").status.srcSet&&d.jPlayer("pause")})},stop:function(){this.status.srcSet?this.html.active?this._html_pause(0):this.flash.active&&this._flash_pause(0):this._urlNotSetError("stop")},playHead:function(a){a=this._limitValue(a,0,100);this.status.srcSet?this.html.active?this._html_playHead(a):this.flash.active&&this._flash_playHead(a):this._urlNotSetError("playHead")},_muted:function(a){this.options.muted=a;this.html.used&&this._html_mute(a);
this.flash.used&&this._flash_mute(a);if(!this.html.video.gate&&!this.html.audio.gate){this._updateMute(a);this._updateVolume(this.options.volume);this._trigger(b.jPlayer.event.volumechange)}},mute:function(a){a=a===f?true:!!a;this._muted(a)},unmute:function(a){a=a===f?true:!!a;this._muted(!a)},_updateMute:function(a){if(a===f)a=this.options.muted;if(this.css.jq.mute.length&&this.css.jq.unmute.length)if(this.status.noVolume){this.css.jq.mute.hide();this.css.jq.unmute.hide()}else if(a){this.css.jq.mute.hide();
this.css.jq.unmute.show()}else{this.css.jq.mute.show();this.css.jq.unmute.hide()}},volume:function(a){a=this._limitValue(a,0,1);this.options.volume=a;this.html.used&&this._html_volume(a);this.flash.used&&this._flash_volume(a);if(!this.html.video.gate&&!this.html.audio.gate){this._updateVolume(a);this._trigger(b.jPlayer.event.volumechange)}},volumeBar:function(a){if(this.css.jq.volumeBar.length){var b=this.css.jq.volumeBar.offset(),d=a.pageX-b.left,e=this.css.jq.volumeBar.width(),a=this.css.jq.volumeBar.height()-
a.pageY+b.top,b=this.css.jq.volumeBar.height();this.options.verticalVolume?this.volume(a/b):this.volume(d/e)}this.options.muted&&this._muted(false)},volumeBarValue:function(a){this.volumeBar(a)},_updateVolume:function(a){if(a===f)a=this.options.volume;a=this.options.muted?0:a;if(this.status.noVolume){this.css.jq.volumeBar.length&&this.css.jq.volumeBar.hide();this.css.jq.volumeBarValue.length&&this.css.jq.volumeBarValue.hide();this.css.jq.volumeMax.length&&this.css.jq.volumeMax.hide()}else{this.css.jq.volumeBar.length&&
this.css.jq.volumeBar.show();if(this.css.jq.volumeBarValue.length){this.css.jq.volumeBarValue.show();this.css.jq.volumeBarValue[this.options.verticalVolume?"height":"width"](a*100+"%")}this.css.jq.volumeMax.length&&this.css.jq.volumeMax.show()}},volumeMax:function(){this.volume(1);this.options.muted&&this._muted(false)},_cssSelectorAncestor:function(a){var c=this;this.options.cssSelectorAncestor=a;this._removeUiClass();this.ancestorJq=a?b(a):[];a&&this.ancestorJq.length!==1&&this._warning({type:b.jPlayer.warning.CSS_SELECTOR_COUNT,
context:a,message:b.jPlayer.warningMsg.CSS_SELECTOR_COUNT+this.ancestorJq.length+" found for cssSelectorAncestor.",hint:b.jPlayer.warningHint.CSS_SELECTOR_COUNT});this._addUiClass();b.each(this.options.cssSelector,function(a,b){c._cssSelector(a,b)})},_cssSelector:function(a,c){var d=this;if(typeof c==="string")if(b.jPlayer.prototype.options.cssSelector[a]){this.css.jq[a]&&this.css.jq[a].length&&this.css.jq[a].unbind(".jPlayer");this.options.cssSelector[a]=c;this.css.cs[a]=this.options.cssSelectorAncestor+
" "+c;this.css.jq[a]=c?b(this.css.cs[a]):[];this.css.jq[a].length&&this.css.jq[a].bind("click.jPlayer",function(c){d[a](c);b(this).blur();return false});c&&this.css.jq[a].length!==1&&this._warning({type:b.jPlayer.warning.CSS_SELECTOR_COUNT,context:this.css.cs[a],message:b.jPlayer.warningMsg.CSS_SELECTOR_COUNT+this.css.jq[a].length+" found for "+a+" method.",hint:b.jPlayer.warningHint.CSS_SELECTOR_COUNT})}else this._warning({type:b.jPlayer.warning.CSS_SELECTOR_METHOD,context:a,message:b.jPlayer.warningMsg.CSS_SELECTOR_METHOD,
hint:b.jPlayer.warningHint.CSS_SELECTOR_METHOD});else this._warning({type:b.jPlayer.warning.CSS_SELECTOR_STRING,context:c,message:b.jPlayer.warningMsg.CSS_SELECTOR_STRING,hint:b.jPlayer.warningHint.CSS_SELECTOR_STRING})},seekBar:function(a){if(this.css.jq.seekBar){var b=this.css.jq.seekBar.offset(),a=a.pageX-b.left,b=this.css.jq.seekBar.width();this.playHead(100*a/b)}},playBar:function(a){this.seekBar(a)},repeat:function(){this._loop(true)},repeatOff:function(){this._loop(false)},_loop:function(a){if(this.options.loop!==
a){this.options.loop=a;this._updateButtons();this._trigger(b.jPlayer.event.repeat)}},currentTime:function(){},duration:function(){},gui:function(){},noSolution:function(){},option:function(a,c){var d=a;if(arguments.length===0)return b.extend(true,{},this.options);if(typeof a==="string"){var e=a.split(".");if(c===f){for(var d=b.extend(true,{},this.options),g=0;g<e.length;g++)if(d[e[g]]!==f)d=d[e[g]];else{this._warning({type:b.jPlayer.warning.OPTION_KEY,context:a,message:b.jPlayer.warningMsg.OPTION_KEY,
hint:b.jPlayer.warningHint.OPTION_KEY});return f}return d}for(var g=d={},h=0;h<e.length;h++)if(h<e.length-1){g[e[h]]={};g=g[e[h]]}else g[e[h]]=c}this._setOptions(d);return this},_setOptions:function(a){var c=this;b.each(a,function(a,b){c._setOption(a,b)});return this},_setOption:function(a,c){var d=this;switch(a){case "volume":this.volume(c);break;case "muted":this._muted(c);break;case "cssSelectorAncestor":this._cssSelectorAncestor(c);break;case "cssSelector":b.each(c,function(a,b){d._cssSelector(a,
b)});break;case "fullScreen":if(this.options[a]!==c){this._removeUiClass();this.options[a]=c;this._refreshSize()}break;case "size":!this.options.fullScreen&&this.options[a].cssClass!==c.cssClass&&this._removeUiClass();this.options[a]=b.extend({},this.options[a],c);this._refreshSize();break;case "sizeFull":this.options.fullScreen&&this.options[a].cssClass!==c.cssClass&&this._removeUiClass();this.options[a]=b.extend({},this.options[a],c);this._refreshSize();break;case "autohide":this.options[a]=b.extend({},
this.options[a],c);this._updateAutohide();break;case "loop":this._loop(c);break;case "nativeVideoControls":this.options[a]=b.extend({},this.options[a],c);this.status.nativeVideoControls=this._uaBlocklist(this.options.nativeVideoControls);this._restrictNativeVideoControls();this._updateNativeVideoControls();break;case "noFullScreen":this.options[a]=b.extend({},this.options[a],c);this.status.nativeVideoControls=this._uaBlocklist(this.options.nativeVideoControls);this.status.noFullScreen=this._uaBlocklist(this.options.noFullScreen);
this._restrictNativeVideoControls();this._updateButtons();break;case "noVolume":this.options[a]=b.extend({},this.options[a],c);this.status.noVolume=this._uaBlocklist(this.options.noVolume);this._updateVolume();this._updateMute();break;case "emulateHtml":if(this.options[a]!==c)(this.options[a]=c)?this._emulateHtmlBridge():this._destroyHtmlBridge()}return this},_refreshSize:function(){this._setSize();this._addUiClass();this._updateSize();this._updateButtons();this._updateAutohide();this._trigger(b.jPlayer.event.resize)},
_setSize:function(){if(this.options.fullScreen){this.status.width=this.options.sizeFull.width;this.status.height=this.options.sizeFull.height;this.status.cssClass=this.options.sizeFull.cssClass}else{this.status.width=this.options.size.width;this.status.height=this.options.size.height;this.status.cssClass=this.options.size.cssClass}this.element.css({width:this.status.width,height:this.status.height})},_addUiClass:function(){this.ancestorJq.length&&this.ancestorJq.addClass(this.status.cssClass)},_removeUiClass:function(){this.ancestorJq.length&&
this.ancestorJq.removeClass(this.status.cssClass)},_updateSize:function(){this.internal.poster.jq.css({width:this.status.width,height:this.status.height});!this.status.waitForPlay&&this.html.active&&this.status.video||this.html.video.available&&this.html.used&&this.status.nativeVideoControls?this.internal.video.jq.css({width:this.status.width,height:this.status.height}):!this.status.waitForPlay&&(this.flash.active&&this.status.video)&&this.internal.flash.jq.css({width:this.status.width,height:this.status.height})},
_updateAutohide:function(){var a=this,b=function(){a.css.jq.gui.fadeIn(a.options.autohide.fadeIn,function(){clearTimeout(a.internal.autohideId);a.internal.autohideId=setTimeout(function(){a.css.jq.gui.fadeOut(a.options.autohide.fadeOut)},a.options.autohide.hold)})};if(this.css.jq.gui.length){this.css.jq.gui.stop(true,true);clearTimeout(this.internal.autohideId);this.element.unbind(".jPlayerAutohide");this.css.jq.gui.unbind(".jPlayerAutohide");if(this.status.nativeVideoControls)this.css.jq.gui.hide();
else if(this.options.fullScreen&&this.options.autohide.full||!this.options.fullScreen&&this.options.autohide.restored){this.element.bind("mousemove.jPlayer.jPlayerAutohide",b);this.css.jq.gui.bind("mousemove.jPlayer.jPlayerAutohide",b);this.css.jq.gui.hide()}else this.css.jq.gui.show()}},fullScreen:function(){this._setOption("fullScreen",true)},restoreScreen:function(){this._setOption("fullScreen",false)},_html_initMedia:function(){this.htmlElement.media.src=this.status.src;this.options.preload!==
"none"&&this._html_load();this._trigger(b.jPlayer.event.timeupdate)},_html_setAudio:function(a){var c=this;b.each(this.formats,function(b,e){if(c.html.support[e]&&a[e]){c.status.src=a[e];c.status.format[e]=true;c.status.formatType=e;return false}});this.htmlElement.media=this.htmlElement.audio;this._html_initMedia()},_html_setVideo:function(a){var c=this;b.each(this.formats,function(b,e){if(c.html.support[e]&&a[e]){c.status.src=a[e];c.status.format[e]=true;c.status.formatType=e;return false}});if(this.status.nativeVideoControls)this.htmlElement.video.poster=
this._validString(a.poster)?a.poster:"";this.htmlElement.media=this.htmlElement.video;this._html_initMedia()},_html_resetMedia:function(){if(this.htmlElement.media){this.htmlElement.media.id===this.internal.video.id&&!this.status.nativeVideoControls&&this.internal.video.jq.css({width:"0px",height:"0px"});this.htmlElement.media.pause()}},_html_clearMedia:function(){if(this.htmlElement.media){this.htmlElement.media.src="";this.htmlElement.media.load()}},_html_load:function(){if(this.status.waitForLoad){this.status.waitForLoad=
false;this.htmlElement.media.load()}clearTimeout(this.internal.htmlDlyCmdId)},_html_play:function(a){var b=this;this._html_load();this.htmlElement.media.play();if(!isNaN(a))try{this.htmlElement.media.currentTime=a}catch(d){this.internal.htmlDlyCmdId=setTimeout(function(){b.play(a)},100);return}this._html_checkWaitForPlay()},_html_pause:function(a){var b=this;a>0?this._html_load():clearTimeout(this.internal.htmlDlyCmdId);this.htmlElement.media.pause();if(!isNaN(a))try{this.htmlElement.media.currentTime=
a}catch(d){this.internal.htmlDlyCmdId=setTimeout(function(){b.pause(a)},100);return}a>0&&this._html_checkWaitForPlay()},_html_playHead:function(a){var b=this;this._html_load();try{if(typeof this.htmlElement.media.seekable==="object"&&this.htmlElement.media.seekable.length>0)this.htmlElement.media.currentTime=a*this.htmlElement.media.seekable.end(this.htmlElement.media.seekable.length-1)/100;else if(this.htmlElement.media.duration>0&&!isNaN(this.htmlElement.media.duration))this.htmlElement.media.currentTime=
a*this.htmlElement.media.duration/100;else throw"e";}catch(d){this.internal.htmlDlyCmdId=setTimeout(function(){b.playHead(a)},100);return}this.status.waitForLoad||this._html_checkWaitForPlay()},_html_checkWaitForPlay:function(){if(this.status.waitForPlay){this.status.waitForPlay=false;this.css.jq.videoPlay.length&&this.css.jq.videoPlay.hide();if(this.status.video){this.internal.poster.jq.hide();this.internal.video.jq.css({width:this.status.width,height:this.status.height})}}},_html_volume:function(a){if(this.html.audio.available)this.htmlElement.audio.volume=
a;if(this.html.video.available)this.htmlElement.video.volume=a},_html_mute:function(a){if(this.html.audio.available)this.htmlElement.audio.muted=a;if(this.html.video.available)this.htmlElement.video.muted=a},_flash_setAudio:function(a){var c=this;try{b.each(this.formats,function(b,d){if(c.flash.support[d]&&a[d]){switch(d){case "m4a":case "fla":c._getMovie().fl_setAudio_m4a(a[d]);break;case "mp3":c._getMovie().fl_setAudio_mp3(a[d]);break;case "rtmpa":c._getMovie().fl_setAudio_rtmp(a[d])}c.status.src=
a[d];c.status.format[d]=true;c.status.formatType=d;return false}});if(this.options.preload==="auto"){this._flash_load();this.status.waitForLoad=false}}catch(d){this._flashError(d)}},_flash_setVideo:function(a){var c=this;try{b.each(this.formats,function(b,d){if(c.flash.support[d]&&a[d]){switch(d){case "m4v":case "flv":c._getMovie().fl_setVideo_m4v(a[d]);break;case "rtmpv":c._getMovie().fl_setVideo_rtmp(a[d])}c.status.src=a[d];c.status.format[d]=true;c.status.formatType=d;return false}});if(this.options.preload===
"auto"){this._flash_load();this.status.waitForLoad=false}}catch(d){this._flashError(d)}},_flash_resetMedia:function(){this.internal.flash.jq.css({width:"0px",height:"0px"});this._flash_pause(NaN)},_flash_clearMedia:function(){try{this._getMovie().fl_clearMedia()}catch(a){this._flashError(a)}},_flash_load:function(){try{this._getMovie().fl_load()}catch(a){this._flashError(a)}this.status.waitForLoad=false},_flash_play:function(a){try{this._getMovie().fl_play(a)}catch(b){this._flashError(b)}this.status.waitForLoad=
false;this._flash_checkWaitForPlay()},_flash_pause:function(a){try{this._getMovie().fl_pause(a)}catch(b){this._flashError(b)}if(a>0){this.status.waitForLoad=false;this._flash_checkWaitForPlay()}},_flash_playHead:function(a){try{this._getMovie().fl_play_head(a)}catch(b){this._flashError(b)}this.status.waitForLoad||this._flash_checkWaitForPlay()},_flash_checkWaitForPlay:function(){if(this.status.waitForPlay){this.status.waitForPlay=false;this.css.jq.videoPlay.length&&this.css.jq.videoPlay.hide();if(this.status.video){this.internal.poster.jq.hide();
this.internal.flash.jq.css({width:this.status.width,height:this.status.height})}}},_flash_volume:function(a){try{this._getMovie().fl_volume(a)}catch(b){this._flashError(b)}},_flash_mute:function(a){try{this._getMovie().fl_mute(a)}catch(b){this._flashError(b)}},_getMovie:function(){return document[this.internal.flash.id]},_checkForFlash:function(a){var b=false,d;if(window.ActiveXObject)try{new ActiveXObject("ShockwaveFlash.ShockwaveFlash."+a);b=true}catch(e){}else if(navigator.plugins&&navigator.mimeTypes.length>
0)(d=navigator.plugins["Shockwave Flash"])&&navigator.plugins["Shockwave Flash"].description.replace(/.*\s(\d+\.\d+).*/,"$1")>=a&&(b=true);return b},_validString:function(a){return a&&typeof a==="string"},_limitValue:function(a,b,d){return a<b?b:a>d?d:a},_urlNotSetError:function(a){this._error({type:b.jPlayer.error.URL_NOT_SET,context:a,message:b.jPlayer.errorMsg.URL_NOT_SET,hint:b.jPlayer.errorHint.URL_NOT_SET})},_flashError:function(a){var c;c=this.internal.ready?"FLASH_DISABLED":"FLASH";this._error({type:b.jPlayer.error[c],
context:this.internal.flash.swf,message:b.jPlayer.errorMsg[c]+a.message,hint:b.jPlayer.errorHint[c]});this.internal.flash.jq.css({width:"1px",height:"1px"})},_error:function(a){this._trigger(b.jPlayer.event.error,a);this.options.errorAlerts&&this._alert("Error!"+(a.message?"\n\n"+a.message:"")+(a.hint?"\n\n"+a.hint:"")+"\n\nContext: "+a.context)},_warning:function(a){this._trigger(b.jPlayer.event.warning,f,a);this.options.warningAlerts&&this._alert("Warning!"+(a.message?"\n\n"+a.message:"")+(a.hint?
"\n\n"+a.hint:"")+"\n\nContext: "+a.context)},_alert:function(a){alert("jPlayer "+this.version.script+" : id='"+this.internal.self.id+"' : "+a)},_emulateHtmlBridge:function(){var a=this;b.each(b.jPlayer.emulateMethods.split(/\s+/g),function(b,d){a.internal.domNode[d]=function(b){a[d](b)}});b.each(b.jPlayer.event,function(c,d){var e=true;b.each(b.jPlayer.reservedEvent.split(/\s+/g),function(a,b){if(b===c)return e=false});e&&a.element.bind(d+".jPlayer.jPlayerHtml",function(){a._emulateHtmlUpdate();
var b=document.createEvent("Event");b.initEvent(c,false,true);a.internal.domNode.dispatchEvent(b)})})},_emulateHtmlUpdate:function(){var a=this;b.each(b.jPlayer.emulateStatus.split(/\s+/g),function(b,d){a.internal.domNode[d]=a.status[d]});b.each(b.jPlayer.emulateOptions.split(/\s+/g),function(b,d){a.internal.domNode[d]=a.options[d]})},_destroyHtmlBridge:function(){var a=this;this.element.unbind(".jPlayerHtml");b.each((b.jPlayer.emulateMethods+" "+b.jPlayer.emulateStatus+" "+b.jPlayer.emulateOptions).split(/\s+/g),
function(b,d){delete a.internal.domNode[d]})}};b.jPlayer.error={FLASH:"e_flash",FLASH_DISABLED:"e_flash_disabled",NO_SOLUTION:"e_no_solution",NO_SUPPORT:"e_no_support",URL:"e_url",URL_NOT_SET:"e_url_not_set",VERSION:"e_version"};b.jPlayer.errorMsg={FLASH:"jPlayer's Flash fallback is not configured correctly, or a command was issued before the jPlayer Ready event. Details: ",FLASH_DISABLED:"jPlayer's Flash fallback has been disabled by the browser due to the CSS rules you have used. Details: ",NO_SOLUTION:"No solution can be found by jPlayer in this browser. Neither HTML nor Flash can be used.",
NO_SUPPORT:"It is not possible to play any media format provided in setMedia() on this browser using your current options.",URL:"Media URL could not be loaded.",URL_NOT_SET:"Attempt to issue media playback commands, while no media url is set.",VERSION:"jPlayer "+b.jPlayer.prototype.version.script+" needs Jplayer.swf version "+b.jPlayer.prototype.version.needFlash+" but found "};b.jPlayer.errorHint={FLASH:"Check your swfPath option and that Jplayer.swf is there.",FLASH_DISABLED:"Check that you have not display:none; the jPlayer entity or any ancestor.",
NO_SOLUTION:"Review the jPlayer options: support and supplied.",NO_SUPPORT:"Video or audio formats defined in the supplied option are missing.",URL:"Check media URL is valid.",URL_NOT_SET:"Use setMedia() to set the media URL.",VERSION:"Update jPlayer files."};b.jPlayer.warning={CSS_SELECTOR_COUNT:"e_css_selector_count",CSS_SELECTOR_METHOD:"e_css_selector_method",CSS_SELECTOR_STRING:"e_css_selector_string",OPTION_KEY:"e_option_key"};b.jPlayer.warningMsg={CSS_SELECTOR_COUNT:"The number of css selectors found did not equal one: ",
CSS_SELECTOR_METHOD:"The methodName given in jPlayer('cssSelector') is not a valid jPlayer method.",CSS_SELECTOR_STRING:"The methodCssSelector given in jPlayer('cssSelector') is not a String or is empty.",OPTION_KEY:"The option requested in jPlayer('option') is undefined."};b.jPlayer.warningHint={CSS_SELECTOR_COUNT:"Check your css selector and the ancestor.",CSS_SELECTOR_METHOD:"Check your method name.",CSS_SELECTOR_STRING:"Check your css selector is a string.",OPTION_KEY:"Check your option name."}})(jQuery);/**
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

org.activemq.AmqAdapter = {

	init: function(options) {
	},

	/**
	 *  Implement this method to make an AJAX call to the AjaxServlet. An
	 *  options object will accompany this class and will contain the properties
	 *  that describe the details of the AJAX call. The options object will
	 *  have the following properties:
	 *
	 *  - method:  'get' or 'post'
	 *  - data:    query data to accompany the post or get.
	 *  - success: A callback function that is invoked upon successful
	 *             completion of the AJAX call. The parameter is:
	 *             - data: The result of the AJAX call. In the case of XML
	 *                     data should resolve to a Document element.
	 *  - error:   A callback when some type of error occurs. The callback
	 *             function's parameters should be:
	 *             - xhr:    The XmlHttpRequest object.
	 *             - status: A text string of the status.
	 *             - ex:     The exception that caused the error.
	 *  - headers: An object containing additional headers for the ajax request.
	 */
	ajax: function(uri, options) {
		request = {
			url: uri,
			data: options.data,
			success: options.success || function(){},
			
                        error: function(xhr, text, e){
                                if(xhr.status == 200) {
                                        var data = document.createElement("ajax-response");
                                        var response = data.getElementsByTagName("ajax-response");
                                        $(data).append("<ajax-response/>");
                                        this.success(data);
                                }else{
                                        options.error(xhr, text, e);
                                }
                        }

		}
		var headers = {};
		if( options.headers ) {
			headers = options.headers;
		}
		
		if (options.method == 'post') {
			request.type = 'POST';
			/* Force "Connection: close" for Mozilla browsers to work around
			 * a bug where XMLHttpReqeuest sends an incorrect Content-length
			 * header. See Mozilla Bugzilla #246651.
			 */
			headers[ 'Connection' ] = 'close';
		} else {
			request.type = 'GET';
			request.dataType = 'xml';
		}
		
		if( headers ) {
			request.beforeSend = function(xhr) {
				for( h in headers ) {
					xhr.setRequestHeader( h, headers[ h ] );
				}
			}
		}
		
		jQuery.ajax( request );
	},

	log: function(message, exception) {
		if (typeof console != 'undefined' && console.log) console.log(message);
	}
};
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
(function($){
	$.amq = {
		connect : function(){
			$.amq._listeners = {};
			$.amq.connected = false;
			$.amq._service = org.activemq.Amq;
			$.amq.subscribe();
			try{
			        $.amq._service.init({
			                uri: '/amq',
			                logging: true,
		        	        timeout: 45,
		                	clientId: (new Date()).getTime().toString(),
					connectStatusHandler : function(connected){
						
						if (!$.amq.connected && connected){
							$.amq._putListeners();
							$.amq.connected = true;
						}else if(!connected){
							$.amq.connected = false;
						}
					},
					sessionInitializedCallback: function(){
						console.info("amq ready.................");
						$.amq._putListeners();
						$.amq.connected = true;
					}
			        });
			}catch(e){
				console.info(e);
			}
		},
		subscribe : function(dest, callback){
			if (this.connected)
				$.amq._addListener(dest, callback);
			if (typeof(this._listeners[dest]) == "undefined"){
				this._listeners[dest] = [];
			}
			this._listeners[dest].push(callback);
		},
		unsubscribe : function(dest){
			 $.amq._service.removeListener('example', dest);
		},
		send : function(dest, message){
			if ($.amq.connected){
				$.amq._service.sendMessage(dest, message);
			}
		},
		connected : false,
		_service : null,
		_putListeners : function(){
			for(var k in $.amq._listeners){
				for(var i=0; i<$.amq._listeners[k].length; i++){
					$.amq._addListener(k, $.amq._listeners[k][i]);
				}
			}
		},
		_listeners : {},
		_addListener: function(dest, callback){
			
		        $.amq._service.addListener('example', dest, callback);
		}
	}
})(jQuery);

