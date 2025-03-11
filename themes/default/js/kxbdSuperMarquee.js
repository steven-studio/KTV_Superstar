/**
 * @classDescription 超级Marquee，可做图片导航，图片轮换
 * @author Aken Li(www.kxbd.com)
 * @date 2009-07-27
 * @dependence jQuery 1.3.2
 * @DOM
 *  	<div id="marquee">
 *  		<ul>
 *   			<li></li>
 *   			<li></li>
 *  		</ul>
 *  	</div>
 * @CSS
 *  	#marquee {width:200px;height:50px;overflow:hidden;}
 * @Usage
 *  	$('#marquee').kxbdSuperMarquee(options);
 * @options
 *		distance:200,
 *		duration:20,
 *		time:5,
 *		direction: 'left',
 *		scrollAmount:1,
 *		scrollDelay:20
 *		isEqual:true,
 *		loop: 0,
 *		btnGo:{left:'#goL',right:'#goR'},
 *		eventGo:'click',
 *		controlBtn:{left:'#goL',right:'#goR'},
 *		newAmount:4,
 *		eventA:'mouseenter',
 *		eventB:'mouseleave',
 *		navId:'#marqueeNav', 
 *		eventNav:'click' 
 */
(function($){

	$.fn.kxbdSuperMarquee = function(options){
		var opts = $.extend({},$.fn.kxbdSuperMarquee.defaults, options);
		
		return this.each(function(){
			var $marquee = $(this);
			var _scrollObj = $marquee.get(0);
			var scrollW = $marquee.width();
			var scrollH = $marquee.height();
			var $element = $marquee.children(); 
			var $kids = $element.children();
			var scrollSize=0;
			var _type = (opts.direction == 'left' || opts.direction == 'right') ? 1:0;
			var scrollId, rollId, isMove, marqueeId;
			var t,b,c,d,e; 
			var _size, _len; 
			var $nav,$navBtns;
			var arrPos = []; 
			var numView = 0; 
			var numRoll=0; 
			var numMoved = 0;

			
			$element.css(_type?'width':'height',10000);
			
			var navHtml = '<ul>';
			if (opts.isEqual) {
				_size = $kids[_type?'outerWidth':'outerHeight']();
				_len = $kids.length;
				scrollSize = _size * _len;
				for(var i=0;i<_len;i++){
					arrPos.push(i*_size);
					navHtml += '<li>'+ (i+1) +'</li>';
				}
			}else{
				$kids.each(function(i){
					arrPos.push(scrollSize);
					scrollSize += $(this)[_type?'outerWidth':'outerHeight']();
					navHtml += '<li>'+ (i+1) +'</li>';
				});
			}
			navHtml += '</ul>';
			
			
			if (scrollSize<(_type?scrollW:scrollH)) return; 
			
			$element.append($kids.clone()).css(_type?'width':'height',scrollSize*2);
			
			
			if (opts.navId) {
				$nav = $(opts.navId).append(navHtml).hover( stop, start );
				$navBtns = $('li', $nav);
				$navBtns.each(function(i){
					$(this).bind(opts.eventNav,function(){
						if(isMove) return;
						if(numView==i) return;
						rollFunc(arrPos[i]);
						$navBtns.eq(numView).removeClass('navOn');
						numView = i;
						$(this).addClass('navOn');
					});
				});
				$navBtns.eq(numView).addClass('navOn');
			}
			
			
			if (opts.direction == 'right' || opts.direction == 'down') {
				_scrollObj[_type?'scrollLeft':'scrollTop'] = scrollSize;
			}else{
				_scrollObj[_type?'scrollLeft':'scrollTop'] = 0;
			}
			
			if(opts.isMarquee){
				
				
				marqueeId = setTimeout(scrollFunc, opts.scrollDelay);
				
				$marquee.hover(
					function(){
						clearInterval(marqueeId);
					},
					function(){
						
						clearInterval(marqueeId);
						marqueeId = setTimeout(scrollFunc, opts.scrollDelay);
					}
				);
				
				
				if(opts.controlBtn){
					$.each(opts.controlBtn, function(i,val){
						$(val).bind(opts.eventA,function(){
							opts.direction = i;
							opts.oldAmount = opts.scrollAmount;
							opts.scrollAmount = opts.newAmount;
						}).bind(opts.eventB,function(){
							opts.scrollAmount = opts.oldAmount;
						});
					});
				}
			}else{
				if(opts.isAuto){
					
					start();
					
					
					$marquee.hover( stop, start );
				}
			
				
				if(opts.btnGo){
					$.each(opts.btnGo, function(i,val){
						$(val).bind(opts.eventGo,function(){
							if(isMove == true) return;
							opts.direction = i;
							rollFunc();
							if (opts.isAuto) {
								stop();
								start();
							}
						});
					});
				}
			}
			
			function scrollFunc(){
				var _dir = (opts.direction == 'left' || opts.direction == 'right') ? 'scrollLeft':'scrollTop';
				
				if(opts.isMarquee){
					if (opts.loop > 0) {
						numMoved+=opts.scrollAmount;
						if(numMoved>scrollSize*opts.loop){
							_scrollObj[_dir] = 0;
							return clearInterval(marqueeId);
						} 
					}
					var newPos = _scrollObj[_dir]+(opts.direction == 'left' || opts.direction == 'up'?1:-1)*opts.scrollAmount;
					
					if (newPos==(scrollSize-$(".marquee").width())){
						$(".marquee").trigger("reload");
						return;
					}
					
				}else{
					if(opts.duration){
						if(t++<d){
							isMove = true;
							var newPos = Math.ceil(easeOutQuad(t,b,c,d));
							if(t==d){
								newPos = e;
							}
						}else{
							newPos = e;
							clearInterval(scrollId);
							isMove = false;
							return;
						}
					}else{
						var newPos = e;
						clearInterval(scrollId);
					}
				}
				
				if(opts.direction == 'left' || opts.direction == 'up'){
					if(newPos>=scrollSize){
						newPos-=scrollSize;
					}
				}else{
					if(newPos<=0){
						newPos+=scrollSize;
					}
				}
				_scrollObj[_dir] = newPos;
				
				if(opts.isMarquee){
					marqueeId = setTimeout(scrollFunc, opts.scrollDelay);
				}else if(t<d){
					if(scrollId) clearTimeout(scrollId);
					scrollId = setTimeout(scrollFunc, opts.scrollDelay);
				}else{
					isMove = false;
				}
				
			};
			
			function rollFunc(pPos){
				isMove = true;
				var _dir = (opts.direction == 'left' || opts.direction == 'right') ? 'scrollLeft':'scrollTop';
				var _neg = opts.direction == 'left' || opts.direction == 'up'?1:-1;

				numRoll = numRoll +_neg;
				
				if(pPos == undefined&&opts.navId){
					$navBtns.eq(numView).removeClass('navOn');
					numView +=_neg;
					if(numView>=_len){
						numView = 0;

					}else if(numView<0){
						numView = _len-1;
					}
					$navBtns.eq(numView).addClass('navOn');
					numRoll = numView;
				}

				var _temp = numRoll<0?scrollSize:0;
				t=0;
				b=_scrollObj[_dir];
				
				e=(pPos != undefined)?pPos:_temp+(opts.distance*numRoll)%scrollSize;
				if(_neg==1){

					if(e>b){
						c = e-b;
					}else{
						c = e+scrollSize -b;
					}
				}else{
					if(e>b){
						c =e-scrollSize-b;
					}else{
						c = e-b;
					}
				}
				d=opts.duration;
				
				
				if(scrollId) clearTimeout(scrollId);
				scrollId = setTimeout(scrollFunc, opts.scrollDelay);
			}
			
			function start(){
				rollId = setInterval(function(){
					rollFunc();
				}, opts.time*1000);
			}
			function stop(){
				clearInterval(rollId);
			}
			
			function easeOutQuad(t,b,c,d){
				return -c *(t/=d)*(t-2) + b;
			}
			
			function easeOutQuint(t,b,c,d){
				return c*((t=t/d-1)*t*t*t*t + 1) + b;
			}

		});
	};
	$.fn.kxbdSuperMarquee.defaults = {
		isMarquee:false,
		isEqual:true,
		loop: 0,
		newAmount:3,
		eventA:'mousedown',
		eventB:'mouseup',
		isAuto:true,
		time:5,
		duration:50,
		eventGo:'click', 
		direction: 'left',
		scrollAmount:1,
		scrollDelay:10,
		eventNav:'click'
	};
	
	$.fn.kxbdSuperMarquee.setDefaults = function(settings) {
		$.extend( $.fn.kxbdSuperMarquee.defaults, settings );
	};
	
})(jQuery);

















