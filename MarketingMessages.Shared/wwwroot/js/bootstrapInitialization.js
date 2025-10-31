function initializeTooltips() {
    let counter = 0;
    function waitForTooltips() {
        const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');

        if (counter < 10 && (!tooltipTriggerList || tooltipTriggerList.length === 0)) {
            console.log('no carousel found, retrying in 1 sec');
            counter += 1;
            setTimeout(() => {
                return waitForTooltips();
            }, 1000)
        }
        else if (tooltipTriggerList.length !== 0) {
            const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]')
            const tooltipList = [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl))
            $(window).on('click', () => tooltipList.map((t) => t.hide()));
        }
        else {
            console.warn('no tooltips were found to initialize, retry timed out.');
        }
    }
    return waitForTooltips();
}

if (document.readyState === 'complete') {
    initializeTooltips();
}
else {
    $(document).on('DOMContentLoaded', initializeTooltips);
}

