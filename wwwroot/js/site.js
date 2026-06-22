// HERO SLIDER
var cur = 0;
var slides = document.querySelectorAll('.slide');
var dots = document.querySelectorAll('.hero-dots span');

function goSlide(n) {
    if (slides.length === 0 || dots.length === 0) return;
    slides[cur].classList.remove('on');
    dots[cur].classList.remove('on');
    cur = n;
    slides[cur].classList.add('on');
    dots[cur].classList.add('on');
}

function changeSlide(d) {
    if (slides.length === 0) return;
    var n = (cur + d + slides.length) % slides.length;
    goSlide(n);
}

if (slides.length > 0) {
    setInterval(function() {
        changeSlide(1);
    }, 5000);
}

// PRODUCT TABS
function filterTab(el, g) {
    if (!el || !el.parentElement) return;
    el.parentElement.querySelectorAll('.ptab').forEach(function(t) {
        t.classList.remove('on');
    });
    el.classList.add('on');
}

// SCROLL ANIMATIONS
var obs = new IntersectionObserver(function(entries) {
    entries.forEach(function(e) {
        if (e.isIntersecting) {
            e.target.style.opacity = '1';
            e.target.style.transform = 'translateY(0)';
        }
    });
}, { threshold: 0.1 });

document.querySelectorAll('.pcard, .ncard, .wcard, .pjcard, .feat').forEach(function(el) {
    el.style.opacity = '0';
    el.style.transform = 'translateY(20px)';
    el.style.transition = 'opacity .5s ease, transform .5s ease';
    obs.observe(el);
});

// COUNTER ANIMATION
function animateCount(el, target) {
    var current = 0;
    var step = Math.ceil(target / 60);
    var t = setInterval(function() {
        current += step;
        if (current >= target) {
            current = target;
            clearInterval(t);
        }
        el.textContent = current.toLocaleString('vi-VN') + (el.dataset.suffix || '');
    }, 25);
}

var cobs = new IntersectionObserver(function(entries) {
    entries.forEach(function(e) {
        if (e.isIntersecting) {
            var num = parseFloat(e.target.dataset.num);
            if (!isNaN(num)) {
                animateCount(e.target, num);
            }
            cobs.unobserve(e.target);
        }
    });
}, { threshold: 0.5 });

document.querySelectorAll('.sitem strong').forEach(function(el, i) {
    var texts = ['2500', '10', '98', '500'];
    var suf = ['+', '+', '%', '+'];
    el.dataset.num = texts[i];
    el.dataset.suffix = suf[i];
    cobs.observe(el);
});
