const eventHandlersMap = new Map();
export function getElement(elemOrSelector) {
    let elem = (typeof elemOrSelector === 'string')
        ? document.getElementById(elemOrSelector)
        : elemOrSelector;
    if (!elem && typeof elemOrSelector === 'string') {
        elem = document.querySelector(elemOrSelector);
    }
    if (!(elem === null || elem === void 0 ? void 0 : elem.getAttribute) && (elem === null || elem === void 0 ? void 0 : elem.id)) {
        elem = document.getElementById(elem.id);
    }
    return elem;
}
export function getElements(elemOrSelector) {
    let elems = (typeof elemOrSelector === 'string')
        ? (document.getElementById(elemOrSelector) || document.querySelectorAll(elemOrSelector))
        : elemOrSelector;
    if (elems instanceof HTMLElement) {
        elems = [elems];
    }
    else if (!(elems instanceof Array)) {
        elems = Array.from(elems);
    }
    if (!(elems === null || elems === void 0 ? void 0 : elems.length) && typeof elemOrSelector === 'string') {
        elems = document.querySelectorAll(elemOrSelector);
    }
    elems = elems.filter(elem => elem !== null && elem !== undefined);
    return elems;
}
export function scrollElementIntoView(elemOrSelector) {
    const element = getElement(elemOrSelector);
    element.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
}
export function synchronizeElementScrolling(sourceElemOrSelector, targetElemOrSelector, synchVertical = true, synchHorizontal = false) {
    const sourceElem = getElement(sourceElemOrSelector);
    const targetElem = getElement(targetElemOrSelector);
    if (sourceElem && targetElem) {
        sourceElem.onscroll = () => {
            if (synchVertical) {
                targetElem.scrollTop = sourceElem.scrollTop;
            }
            if (synchHorizontal) {
                targetElem.scrollLeft = sourceElem.scrollLeft;
            }
        };
        if (synchVertical) {
            targetElem.scrollTop = sourceElem.scrollTop;
        }
        if (synchHorizontal) {
            targetElem.scrollLeft = sourceElem.scrollLeft;
        }
    }
}
export function removeScrollSynchronization(sourceElemOrSelector) {
    const sourceElem = getElement(sourceElemOrSelector);
    if (sourceElem) {
        sourceElem.onscroll = null;
    }
}
export function synchronizeTableRowHover(sourceTable, targetTable) {
    const sourceElem = getElement(sourceTable);
    const targetElem = getElement(targetTable);
    if (targetElem && sourceElem) {
        const sourceRows = sourceElem.querySelectorAll('tr');
        const targetRows = targetElem.querySelectorAll('tr');
        sourceRows.forEach((sourceRow, i) => {
            const targetRow = targetRows[i];
            if (targetRow) {
                const mouseOverHandler = (e) => handleAndCloneMouseEvent(e, sourceRow, targetRow);
                const mouseOutHandler = (e) => handleAndCloneMouseEvent(e, sourceRow, targetRow);
                sourceRow.addEventListener('mouseover', mouseOverHandler);
                sourceRow.addEventListener('mouseout', mouseOutHandler);
                eventHandlersMap.set(sourceRow, {
                    mouseover: mouseOverHandler,
                    mouseout: mouseOutHandler
                });
            }
        });
    }
}
export function removeTableRowHoverSynchronization(sourceTable) {
    const sourceElem = getElement(sourceTable);
    if (sourceElem) {
        const sourceRows = sourceElem.querySelectorAll('tr');
        sourceRows.forEach(sourceRow => {
            const handlers = eventHandlersMap.get(sourceRow);
            if (handlers) {
                sourceRow.removeEventListener('mouseover', handlers.mouseover);
                sourceRow.removeEventListener('mouseout', handlers.mouseout);
                eventHandlersMap.delete(sourceRow);
            }
        });
    }
}
export function synchronizeElementMouseEvents(sourceElemOrSelector, targetElemOrSelector) {
    const sourceElem = getElement(sourceElemOrSelector);
    const targetElem = getElement(targetElemOrSelector);
    if (targetElem && sourceElem) {
        const mouseOverHandler = (e) => handleAndCloneMouseEvent(e, sourceElem, targetElem);
        const mouseOutHandler = (e) => handleAndCloneMouseEvent(e, sourceElem, targetElem);
        sourceElem.addEventListener('mouseover', mouseOverHandler);
        sourceElem.addEventListener('mouseout', mouseOutHandler);
        eventHandlersMap.set(sourceElem, {
            mouseover: mouseOverHandler,
            mouseout: mouseOutHandler
        });
    }
}
export function removeScrollMouseEventsSynchronization(sourceElemOrSelector) {
    const sourceElem = getElement(sourceElemOrSelector);
    if (sourceElem) {
        const handlers = eventHandlersMap.get(sourceElem);
        if (handlers) {
            sourceElem.removeEventListener('mouseover', handlers.mouseover);
            sourceElem.removeEventListener('mouseout', handlers.mouseout);
            eventHandlersMap.delete(sourceElem);
        }
    }
}
export function elementExists(elemOrSelector) {
    return !!getElement(elemOrSelector);
}
function handleAndCloneMouseEvent(event, elem1, elem2) {
    var _a, _b, _c;
    const origEvent = event;
    const originalElement = (_a = origEvent === null || origEvent === void 0 ? void 0 : origEvent.customDetail) === null || _a === void 0 ? void 0 : _a.originalTarget;
    if (originalElement === event.currentTarget) {
        return;
    }
    var curTarget = event.currentTarget;
    if (event.type === 'mouseover' && !curTarget.classList.contains('force-hover-style')) {
        curTarget.classList.add('force-hover-style');
    }
    else if (event.type === 'mouseout' && curTarget.classList.contains('force-hover-style')) {
        curTarget.classList.remove('force-hover-style');
    }
    const clonedEvent = new MouseEvent(event.type, {
        clientX: event.clientX,
        clientY: event.clientY,
        bubbles: true,
        cancelable: true,
    });
    clonedEvent.customDetail = { originalTarget: (_c = (_b = origEvent === null || origEvent === void 0 ? void 0 : origEvent.customDetail) === null || _b === void 0 ? void 0 : _b.originalTarget) !== null && _c !== void 0 ? _c : event.currentTarget };
    event.currentTarget === elem1
        ? elem2.dispatchEvent(clonedEvent)
        : elem1.dispatchEvent(clonedEvent);
}
