export default {
  defaultTheme: 'light',
  iconLinks: [
    {
      icon: 'github',
      href: 'https://github.com/keremvaris/Intentum',
      title: 'Intentum on GitHub',
    },
  ],
  start: () => {
    // Sidebar TOC accordion: collapse groups by default, expand on click; keep current page's group open
    const storageKey = 'intentum-toc-expanded';
    const getStored = () => {
      try {
        const raw = localStorage.getItem(storageKey);
        return raw ? new Set(JSON.parse(raw)) : null;
      } catch {
        return null;
      }
    };
    const setStored = (set) => {
      try {
        localStorage.setItem(storageKey, JSON.stringify([...set]));
      } catch {}
    };

    // Left sidebar TOC: modern template may use .affix, or a sidebar/nav container
    const nav =
      document.querySelector('.affix .nav') ||
      document.querySelector('[class*="sidebar"] .nav') ||
      document.querySelector('.navbar-nav') ||
      Array.from(document.querySelectorAll('.nav')).find((n) => n.querySelectorAll(':scope > li > ul').length > 0);
    if (!nav) return;
    const topLevel = nav.querySelectorAll(':scope > li');
    const groups = [];
    topLevel.forEach((li) => {
      const childUl = li.querySelector(':scope > ul');
      if (!childUl) return;
      li.classList.add('toc-group');
      const anchor = li.querySelector(':scope > a');
      const name = (anchor && anchor.textContent && anchor.textContent.trim()) || '';
      groups.push({ li, name });

      const isActive = li.querySelector('.active');
      const stored = getStored();
      const expandedByStorage = stored && stored.has(name);
      if (isActive || expandedByStorage) li.classList.add('expanded');

      if (anchor) {
        anchor.addEventListener('click', (e) => {
          e.preventDefault();
          li.classList.toggle('expanded');
          const expandedEls = nav.querySelectorAll(':scope > li.toc-group.expanded');
          const openNames = new Set(
            Array.from(expandedEls)
              .map((el) => el.querySelector(':scope > a')?.textContent?.trim())
              .filter(Boolean)
          );
          setStored(openNames);
        });
      }
    });
  },
};
