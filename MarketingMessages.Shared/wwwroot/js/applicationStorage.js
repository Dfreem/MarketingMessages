function saveToSession(key, value) {
    window.sessionStorage.setItem(key, value);
}

function getFromSession(key) {
    return window.sessionStorage.getItem(key);
}
function clearSession() {
    window.sessionStorage.clear();
}

function removeFromSession(key) {
    window.sessionStorage.removeItem(key);
}

function saveToLocal(key, value) {
    window.localStorage.setItem(key, value);
}
function getFromLocal(key) {
    return window.sessionStorage.getItem(key)
}

