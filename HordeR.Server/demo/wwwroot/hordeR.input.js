const keybindings = {};
const actionbindings = {};
const actionStates = {};
let monitoring = false;

export function getButtonCode(button) {
    switch (button) {
        case 0:
            return "LeftMouse";
        case 1:
            return "MiddleMouse";
        case 2:
            return "RightMouse";
        default:
            return "Unknown";
    }
}

export function addKeyBinding(key, action) {
    if (!keybindings[key]) {
        keybindings[key] = [];
    }

    if (!actionbindings[action]) {
        actionbindings[action] = [];
    }

    keybindings[key].push(action);
    actionbindings[action].push(key);
    actionStates[action] = { pressed: false, previousState: false };
}

export function removeKeyBinding(key, action) {
    const keybinding = keybindings[key];
    if (keybinding) {
        const index = keybinding.indexOf(action);
        if (index > -1) {
            actionStates[action] = { pressed: false, previousState: false };
            keybinding.splice(index, 1);
        }
    }

    const actionbinding = actionbindings[action];
    if (actionbinding) {
        const index = actionbinding.indexOf(key);
        if (index > -1) {
            actionbinding.splice(index, 1);
        }
    }
}

export function changeKeyBinding(key, action, newKey) {
    removeKeyBinding(key, action);
    addKeyBinding(newKey, action);
}

export function isKeyPressed(action) {
    return actionStates[action]?.pressed;
}

export function isKeyReleased(action) {
    return !actionStates[action]?.pressed;
}

function isInputElement(element) {
    return element.tagName === "INPUT"
        || element.tagName === "SELECT"
        || element.tagName === "BUTTON"
        || element.tagName === "TEXTAREA";
}

function keydown(e) {
    if (!isInputElement(e.target)) {
        e.stopPropagation();
        e.preventDefault();
    }
    const keybinding = keybindings[e.code];
    if (keybinding) {
        for (let i in keybinding) {
            actionStates[keybinding[i]].previousState = actionStates[keybinding[i]].pressed;
            actionStates[keybinding[i]].pressed = true;
        }
    }
}

function keyup(e) {
    if (!isInputElement(e.target)) {
        e.stopPropagation();
        e.preventDefault();
    }
    const keybinding = keybindings[e.code];
    if (keybinding) {
        for (let i in keybinding) {
            actionStates[keybinding[i]].previousState = actionStates[keybinding[i]].pressed;
            actionStates[keybinding[i]].pressed = false;
        }
    }
}

function mousedown(e) {
    if (!isInputElement(e.target)) {
        e.stopPropagation();
        e.preventDefault();
    }

    const button = getButtonCode(e.button);

    const keybinding = keybindings[button];
    if (keybinding) {
        for (let i in keybinding) {
            actionStates[keybinding[i]].previousState = actionStates[keybinding[i]].pressed;
            actionStates[keybinding[i]].pressed = true;
        }
    }
}

function mouseup(e) {
    if (!isInputElement(e.target)) {
        e.stopPropagation();
        e.preventDefault();
    }

    const button = getButtonCode(e.button);

    const keybinding = keybindings[button];
    if (keybinding) {
        for (let i in keybinding) {
            actionStates[keybinding[i]].previousState = actionStates[keybinding[i]].pressed;
            actionStates[keybinding[i]].pressed = false;
        }
    }
}

export function startMonitoringKeyboard() {
    if (monitoring) {
        return;
    }
    monitoring = true;
    document.addEventListener("keyup", keyup);
    document.addEventListener("keydown", keydown);
    document.addEventListener("mousedown", mousedown);
    document.addEventListener("mouseup", mouseup);
}

export function stopMonitoringKeyboard() {
    if (!monitoring) {
        return;
    }
    monitoring = false;
    document.removeEventListener("keyup", keyup);
    document.removeEventListener("keydown", keydown);
}

export function bindings() {
    return { keys: keybindings, actions: actionbindings };
}

export function print() {
    console.log(keybindings);
}