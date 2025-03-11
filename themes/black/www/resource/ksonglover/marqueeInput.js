define(function() {
    return {
        showMarqueeInput: function() {
            document.getElementById('btnmarquee').style.display = 'block';
        },
        submitMarquee: function() {
            var marqueeText = document.getElementById('marquee-input').value;
            console.log('跑馬燈：', marqueeText);
            
        }
    };
});
