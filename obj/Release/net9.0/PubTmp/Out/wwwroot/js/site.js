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
  const isOpen = !panel.hasAttribute('hidden');
  const willExpand = (typeof expand === 'boolean') ? expand : !isOpen;

  if (willExpand) {
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
      panel.removeEventListener('transitionend', onEnd);
    };
    panel.addEventListener('transitionend', onEnd);
  } else {
    panel.style.height = panel.scrollHeight + 'px';
    // Forzar reflow
    // eslint-disable-next-line no-unused-expressions
    panel.offsetHeight;
    panel.style.height = '0px';

    const onEnd = () => {
      panel.setAttribute('hidden', '');
      panel.style.height = '0px';
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

  if (!trigger || !panel) return;

  // Estado inicial
  panel.classList.add('collapse-slide');
  panel.setAttribute('hidden', '');
  panel.style.height = '0px';
  trigger.setAttribute('aria-expanded', 'false');

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
