export function registerCollapseEvents(dotnetRef, onClose, onOpen) {

    let counter = 0;
    const register = () => {
        counter += 1;
        const modals = document.querySelectorAll('.collapse');
        if (!modals && counter < 3) {
            setTimeout(1000, register);
        }
        else {
            Array.from(modals).map((m) => {
                m.addEventListener('shown.bs.collapse', (e) => {
                    dotnetRef.invokeMethodAsync(onClose);
                });
            });
            Array.from(modals).map((m) => {
                m.addEventListener('hidden.bs.collapse', (e) => {
                    dotnetRef.invokeMethodAsync(onOpen);
                });
            });
        }
    };
    register();
}

export function collapse(id) {
    const collapse = document.getElementById(id);
    const bsCollapse = new bootstrap.Collapse(collapse, {
        toggle: false
    });
    bsCollapse.hide();
    console.log('collapsed', bsCollapse);
};

export function expand(id, closeOthers = false) {
    if (closeOthers) {
        Array.from(document.querySelectorAll('.collapse')).map((m) => {
            new bootstrap.Collapse(m, {toggle: false}).hide();
        });
    }
    const bsCollapse = new bootstrap.Collapse(`#${id}`, {
        toggle: false
    });
    bsCollapse.show();
};