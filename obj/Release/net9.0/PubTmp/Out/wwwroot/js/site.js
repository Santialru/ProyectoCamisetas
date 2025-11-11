// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Make Catalog dropdown title navigate to catalog on click

// document.addEventListener('DOMContentLoaded', function () {
//   var el = document.getElementById('catalogoDropdown');
//   if (!el) return;
//   el.addEventListener('click', function (e) {
//     // If user uses modifier keys or middle click, let browser handle
//     if (e.button !== 0 || e.metaKey || e.ctrlKey || e.shiftKey || e.altKey) return;
//     e.preventDefault();
//     e.stopPropagation();
//     var href = el.getAttribute('href');
//     if (href) window.location.assign(href);
//   });
// });

// Animación suave: abre/cierra un panel con transición de height
function slideToggle(panel, expand) {
  if (!panel) return;
  if (panel.dataset.animating === '1') return; // evita estados inconsistentes por clicks rápidos
  const isOpen = !panel.hasAttribute('hidden');
  const willExpand = (typeof expand === 'boolean') ? expand : !isOpen;

  if (willExpand) {
    panel.dataset.animating = '1';
    panel.removeAttribute('hidden');
    panel.style.height = 'auto';
    const target = panel.scrollHeight + 'px';
    panel.style.height = '0px';
    // Forzar reflow
    // eslint-disable-next-line no-unused-expressions
    panel.offsetHeight;
    panel.style.height = target;

    const onEnd = () => {
      panel.style.height = 'auto';
      delete panel.dataset.animating;
      panel.removeEventListener('transitionend', onEnd);
    };
    panel.addEventListener('transitionend', onEnd);
  } else {
    panel.dataset.animating = '1';
    panel.style.height = panel.scrollHeight + 'px';
    // Forzar reflow
    // eslint-disable-next-line no-unused-expressions
    panel.offsetHeight;
    panel.style.height = '0px';

    const onEnd = () => {
      panel.setAttribute('hidden', '');
      panel.style.height = '0px';
      delete panel.dataset.animating;
      panel.removeEventListener('transitionend', onEnd);
    };
    panel.addEventListener('transitionend', onEnd);
  }
}

// Cerrar al hacer clic fuera
function clickOutside(targets, cb) {
  document.addEventListener('click', (e) => {
    const inside = targets.some(t => t && (t === e.target || t.contains(e.target)));
    if (!inside) cb();
  });
}

document.addEventListener('DOMContentLoaded', function () {
  const trigger = document.getElementById('catalogoDropdown');
  const panel   = document.getElementById('catalogoPanel');
  const navCollapse = document.getElementById('navbarSupportedContent');
  const isTouch = (window.matchMedia && window.matchMedia('(hover: none)').matches) || ('ontouchstart' in window);

  if (!trigger || !panel) return;

  // Estado inicial
  panel.classList.add('collapse-slide');
  panel.setAttribute('hidden', '');
  panel.style.height = '0px';
  trigger.setAttribute('aria-expanded', 'false');

  // Hover abre/cierra en desktop
  if (!isTouch) {
    let overPanel = false;
    trigger.addEventListener('mouseenter', () => {
      if (panel.hasAttribute('hidden')) {
        slideToggle(panel, true);
        trigger.setAttribute('aria-expanded', 'true');
      }
    });
    // be more forgiving moving from trigger into panel/flyout
    trigger.addEventListener('mouseleave', () => {
      setTimeout(() => {
        if (!overPanel && !panel.hasAttribute('hidden')) {
          slideToggle(panel, false);
          trigger.setAttribute('aria-expanded', 'false');
        }
      }, 250);
    });
    panel.addEventListener('mouseenter', () => { overPanel = true; });
    panel.addEventListener('mouseleave', (e) => {
      overPanel = false;
      const toEl = e.relatedTarget;
      // if moving into any element within panel (including flyouts), don't close
      if (toEl && (panel.contains(toEl) || (trigger && trigger.contains(toEl)))) return;
      if (!panel.hasAttribute('hidden')) {
        slideToggle(panel, false);
        trigger.setAttribute('aria-expanded', 'false');
      }
    });
  }

  // Captura clic para priorizar navegación/tap-intent por encima del listener antiguo
  trigger.addEventListener('click', (e) => {
    if (e.button !== 0 || e.metaKey || e.ctrlKey || e.shiftKey || e.altKey) return;
    if (!isTouch) {
      // Desktop: cerrar panel y permitir navegar (no preventDefault)
      if (!panel.hasAttribute('hidden')) {
        slideToggle(panel, false);
        trigger.setAttribute('aria-expanded', 'false');
      }
      e.stopImmediatePropagation();
      return; // dejar que el enlace navegue
    }
    // Touch: primer toque abre, segundo navega
    if (panel.hasAttribute('hidden')) {
      e.preventDefault();
      e.stopImmediatePropagation();
      slideToggle(panel, true);
      trigger.setAttribute('aria-expanded', 'true');
      trigger.dataset.tapOnce = '1';
      setTimeout(() => { delete trigger.dataset.tapOnce; }, 1500);
    } else if (trigger.dataset.tapOnce === '1') {
      // segundo toque: permitir navegación
      delete trigger.dataset.tapOnce;
      e.stopImmediatePropagation(); // cancelar el viejo handler que haría preventDefault
      // no hacemos preventDefault para que el enlace navegue
    }
  }, true);

  // Captura teclado: Space abre/cierra; Enter navega
  trigger.addEventListener('keydown', (e) => {
    if (e.key === ' ') {
      e.preventDefault();
      e.stopImmediatePropagation();
      const willExpand = panel.hasAttribute('hidden');
      slideToggle(panel, willExpand);
      trigger.setAttribute('aria-expanded', String(willExpand));
    } else if (e.key === 'Enter') {
      e.preventDefault();
      e.stopImmediatePropagation();
      // cerrar panel si estaba abierto y navegar
      if (!panel.hasAttribute('hidden')) {
        slideToggle(panel, false);
        trigger.setAttribute('aria-expanded', 'false');
      }
      const href = trigger.getAttribute('href');
      if (href) window.location.assign(href);
    }
  }, true);

  // Toggle: primer clic abre, segundo clic cierra
  trigger.addEventListener('click', (e) => {
    // Permitir Ctrl/Cmd/Shift/Alt o botón medio para abrir en nueva pestaña si quisieras
    if (e.button !== 0 || e.metaKey || e.ctrlKey || e.shiftKey || e.altKey) return;

    e.preventDefault();
    e.stopPropagation();

    const willExpand = panel.hasAttribute('hidden');
    slideToggle(panel, willExpand);
    trigger.setAttribute('aria-expanded', String(willExpand));
  });

  // Teclado accesible
  trigger.addEventListener('keydown', (e) => {
    if (e.key === 'Enter' || e.key === ' ') {
      e.preventDefault();
      trigger.click();
    }
  });

  // Cerrar con ESC
  document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape' && !panel.hasAttribute('hidden')) {
      slideToggle(panel, false);
      trigger.setAttribute('aria-expanded', 'false');
    }
  });

  // Cerrar al hacer clic fuera
  clickOutside([trigger, panel], () => {
    if (!panel.hasAttribute('hidden')) {
      slideToggle(panel, false);
      trigger.setAttribute('aria-expanded', 'false');
    }
  });

  // Cerrar el panel si se colapsa el menú hamburguesa
  try {
    if (navCollapse) {
      navCollapse.addEventListener('hide.bs.collapse', () => {
        if (!panel.hasAttribute('hidden')) {
          slideToggle(panel, false);
          trigger.setAttribute('aria-expanded', 'false');
        }
      });
    }
  } catch (_) { /* ignore */ }

  // Al seleccionar una opción del catálogo: cerrar panel y (en móvil) colapsar navbar
  panel.addEventListener('click', (e) => {
    const link = e.target && e.target.closest ? e.target.closest('a.dropdown-item') : null;
    if (!link) return;
    slideToggle(panel, false);
    trigger.setAttribute('aria-expanded', 'false');
    try {
      if (navCollapse && navCollapse.classList.contains('show') && window.bootstrap && window.bootstrap.Collapse) {
        const instance = window.bootstrap.Collapse.getOrCreateInstance(navCollapse);
        instance.hide();
      }
    } catch (_) { /* ignore */ }
  });
});

// (removed) Catalog infinite scroll



// Inject a global mobile search form into main when navbar is collapsed
document.addEventListener('DOMContentLoaded', function () {
  try {
    var isMobile = window.matchMedia('(max-width: 576px)').matches;
    if (!isMobile) return;
    var main = document.querySelector('main[role="main"]');
    if (!main) return;
    // Avoid duplicates if a page already provides a main search
    if (main.querySelector('input[name="q"]')) return;

    var form = document.createElement('form');
    form.method = 'get';
    form.action = '/Camisetas/Index';
    form.className = 'row g-2 mb-3 mobile-site-search';

    var colInput = document.createElement('div');
    colInput.className = 'col-9';
    var input = document.createElement('input');
    input.type = 'text';
    input.name = 'q';
    input.placeholder = 'Buscar camisetas...';
    input.className = 'form-control';
    var qs = new URLSearchParams(window.location.search).get('q');
    if (qs) input.value = qs;
    colInput.appendChild(input);

    var colBtn = document.createElement('div');
    colBtn.className = 'col-3';
    var btn = document.createElement('button');
    btn.type = 'submit';
    btn.className = 'btn btn-primary w-100';
    btn.textContent = 'Buscar';
    colBtn.appendChild(btn);

    form.appendChild(colInput);
    form.appendChild(colBtn);


    // Sticky placement just under navbar
    var nav = document.querySelector('header .navbar');
    var top = nav ? nav.getBoundingClientRect().height : 56;
    form.style.position = 'sticky';
    form.style.top = top + 'px';
    form.style.zIndex = 1045;
    form.style.background = '#0f1625';
    form.style.borderBottom = '1px solid #2b3448';
    form.style.padding = '8px 0';
    form.style.marginBottom = '8px';

    main.insertBefore(form, main.firstChild);
  } catch (e) { /* no-op */ }
});

// (removed) hero arrows; use dot indicators only

// Position carousel arrows at the image/container edges (no inset)
document.addEventListener('DOMContentLoaded', function () {
  function positionArrows(carousel) {
    if (!carousel) return;
    var prev = carousel.querySelector('.carousel-control-prev');
    var next = carousel.querySelector('.carousel-control-next');
    if (!prev || !next) return;
    // Reset any JS-set inline styles and pin to container edges
    prev.style.left = '0px';
    prev.style.right = '';
    next.style.right = '0px';
    next.style.left = '';
  }

  function wireCarousel(carousel) {
    if (!carousel) return;
    positionArrows(carousel);
    window.addEventListener('resize', function () { positionArrows(carousel); });
    try {
      carousel.addEventListener('slid.bs.carousel', function () { positionArrows(carousel); });
    } catch (_) { /* ignore */ }
  }

  // Details carousels and hero carousel
  document.querySelectorAll('.details-media .carousel, #heroCarousel').forEach(wireCarousel);
});

// Enable touch swipe on carousels for mobile
document.addEventListener('DOMContentLoaded', function () {
  try {
    function wireSwipe(carousel) {
      if (!carousel) return;
      var startX = 0, startY = 0, dx = 0, dy = 0, moved = false;
      function onStart(e) {
        var t = e.touches && e.touches[0];
        if (!t) return;
        startX = t.clientX; startY = t.clientY; dx = 0; dy = 0; moved = false;
      }
      function onMove(e) {
        var t = e.touches && e.touches[0];
        if (!t) return;
        dx = t.clientX - startX; dy = t.clientY - startY;
        if (!moved && Math.abs(dx) > Math.abs(dy) && Math.abs(dx) > 20) {
          moved = true;
        }
      }
      function onEnd() {
        if (!moved) return;
        var inst = null;
        try { inst = window.bootstrap && window.bootstrap.Carousel ? window.bootstrap.Carousel.getOrCreateInstance(carousel) : null; } catch (_) {}
        if (!inst) return;
        if (dx < 0) { inst.next(); } else { inst.prev(); }
      }
      carousel.addEventListener('touchstart', onStart, { passive: true });
      carousel.addEventListener('touchmove', onMove, { passive: true });
      carousel.addEventListener('touchend', onEnd, { passive: true });
    }
    document.querySelectorAll('#heroCarousel, .details-media .carousel').forEach(wireSwipe);
  } catch (_) { /* ignore */ }
});

// Autocomplete for search inputs (name="q")
document.addEventListener('DOMContentLoaded', function () {
  try {
    var inputs = document.querySelectorAll('input[name="q"]');
    inputs.forEach(function (input) { wireAutocomplete(input); });

    // If a mobile search form is dynamically injected later, re-scan
    setTimeout(function(){
      var more = document.querySelectorAll('input[name="q"]');
      more.forEach(function (el) { if (!el.dataset.autoWired) wireAutocomplete(el); });
    }, 800);

    function wireAutocomplete(input) {
      if (!input || input.dataset.autoWired) return;
      input.dataset.autoWired = '1';
      var form = input.closest('form');
      var parent = input.parentElement;
      if (getComputedStyle(parent).position === 'static') { parent.style.position = 'relative'; }
      // Avoid native browser autocomplete interfering
      try { input.setAttribute('autocomplete', 'off'); } catch(_) {}

      var menu = document.createElement('div');
      menu.className = 'autocomplete-menu';
      menu.setAttribute('role', 'listbox');
      menu.hidden = true;
      parent.appendChild(menu);
      // Place menu right under the input
      try { menu.style.top = (input.offsetHeight + 4) + 'px'; } catch(_) {}

      var timer = null;
      var aborter = null;
      var lastTerm = '';
      var activeIndex = -1;
      var items = [];

      input.addEventListener('input', function () {
        var q = input.value.trim();
        clearTimeout(timer);
        if (q.length < 2) {
          try { if (aborter) aborter.abort(); } catch (_) {}
          hide();
          return;
        }
        timer = setTimeout(function () { lastTerm = q; fetchSuggestions(q); }, 160);
        // recalc position in case of layout changes
        try { menu.style.top = (input.offsetHeight + 4) + 'px'; } catch(_) {}
      });
      // For type="search": native clear (x) triggers 'search'
      input.addEventListener('search', function(){
        var q = (input.value || '').trim();
        if (q.length < 2) {
          try { if (aborter) aborter.abort(); } catch (_) {}
          hide();
        }
      });
      window.addEventListener('resize', function(){ try { menu.style.top = (input.offsetHeight + 4) + 'px'; } catch(_) {} });
      input.addEventListener('keydown', function (e) {
        if (menu.hidden) return;
        if (e.key === 'ArrowDown') { e.preventDefault(); setActive(activeIndex + 1); }
        else if (e.key === 'ArrowUp') { e.preventDefault(); setActive(activeIndex - 1); }
        else if (e.key === 'Enter') { if (activeIndex >= 0) { e.preventDefault(); choose(items[activeIndex]); } }
        else if (e.key === 'Escape') { hide(); }
      });
      input.addEventListener('blur', function(){ setTimeout(hide, 150); });

      menu.addEventListener('mousedown', function (e) {
        var el = e.target.closest('[data-value]');
        if (!el) return;
        choose({ value: el.dataset.value });
      });

      function fetchSuggestions(q) {
        try { if (aborter) aborter.abort(); } catch (_) {}
        aborter = new AbortController();
        fetch('/api/camisetas/suggest?q=' + encodeURIComponent(q), { signal: aborter.signal })
          .then(function (r) { return r.ok ? r.json() : []; })
          .then(function (data) { render(data || [], q); })
          .catch(function () { /* ignore */ });
      }

      function render(list, requestedTerm) {
        var current = (input.value || '').trim();
        if (current.length < 2 || (requestedTerm && requestedTerm !== current)) {
          hide();
          return;
        }
        items = list;
        if (!items.length) { hide(); return; }
        activeIndex = -1;
        menu.innerHTML = items.map(function (i, idx) {
          return '<div class="autocomplete-item" role="option" data-index="' + idx + '" data-value="' + escapeHtml(i.value) + '">' +
                   '<span class="pill pill-' + i.type + '">' + i.type + '</span> ' + escapeHtml(i.value) +
                 '</div>';
        }).join('');
        menu.hidden = false;
      }

      function setActive(idx) {
        var els = menu.querySelectorAll('.autocomplete-item');
        if (!els.length) return;
        if (idx < 0) idx = els.length - 1;
        if (idx >= els.length) idx = 0;
        els.forEach(function (el) { el.classList.remove('active'); });
        els[idx].classList.add('active');
        activeIndex = idx;
      }

      function choose(item) {
        if (!item) return;
        input.value = item.value;
        hide();
        if (form) form.submit();
      }

      function hide() {
        menu.hidden = true; menu.innerHTML = ''; items = []; activeIndex = -1;
      }
    }

    function escapeHtml(s) {
      return String(s).replace(/[&<>"']/g, function (c) {
        return ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' })[c] || c;
      });
    }
  } catch (_) { /* ignore */ }
});
