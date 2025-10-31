export function showModal(id) {
    try {
        const elementRef = document.getElementById(id);

        if (elementRef == null) {
            console.warn('unable to find element with id', id);
            return;
        }
        console.log("showing modal", elementRef);
        if (elementRef == null) {
            console.error("element reference in modal.js is null");
            return;
        }

        // hide any open modal first
        Array.from(document.getElementsByClassName('modal')).map((e) => {
            let modal = new bootstrap.Modal(e);
            modal.hide();
        });

        // then open the modal we want
        const visibleModal = new bootstrap.Modal(elementRef);
        visibleModal.show();
        //document.getElementsByTagName('body')[0].style.paddingRight = 0;
    }
    catch (err) {
        console.error('an error occurred while showing modal', err);
    }
}

export function hideModal(id) {
    const elementRef = document.getElementById(id);
    if (elementRef == null) return;
    let modal = bootstrap.Modal.getOrCreateInstance(elementRef);
    if (!modal) {
        modal = new bootstrap.Modal(elementRef);
    }
    if (!modal) return;
    modal.hide();
    //document.getElementsByTagName('body')[0].style.paddingRight = 0;
}

export function registerModalEvents(id, dotnetRef, callOnShow, callOnHide) {
    let counter = 0;
    const register = () => {
        counter += 1;
        const modal = document.getElementById(id);
        if (!modal && counter < 3) {
            setTimeout(register, 1000);
        }
        if (!modal) return;
        else {
            modal.addEventListener('shown.bs.modal', (e) => {
                if (dotnetRef && callOnShow) {
                    dotnetRef.invokeMethodAsync(callOnShow);
                }
            });
            modal.addEventListener('hidden.bs.modal', (e) => {
                if (dotnetRef && callOnHide) {
                    dotnetRef.invokeMethodAsync(callOnHide);
                }
            });
            modal.addEventListener('hidePrevented.bs.modal', (e) => {
                if (dotnetRef && callOnHide) {
                    dotnetRef.invokeMethodAsync(callOnHide);
                }
            });
        }
    };
    register();
}
