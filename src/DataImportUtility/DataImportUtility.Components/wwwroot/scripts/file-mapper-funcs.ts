const eventHandlersMap = new Map<HTMLElement, { [key: string]: EventListener }>();

interface ExtendedMouseEvent extends MouseEvent {
    customDetail?: {
        originalTarget?: EventTarget;
    };
}

export function getElement(elemOrSelector: string | HTMLElement): HTMLElement {
    let elem = (typeof elemOrSelector === 'string')
        ? document.getElementById(elemOrSelector)
        : elemOrSelector;

    if (!elem && typeof elemOrSelector === 'string') { elem = document.querySelector(elemOrSelector); }

    if (!elem?.getAttribute && elem?.id) {
        elem = document.getElementById(elem.id);
    }
    return elem;
}

export function getElements(elemOrSelector: string | HTMLElement | HTMLElement[]): HTMLElement[] {
    let elems = (typeof elemOrSelector === 'string')
        ? (document.getElementById(elemOrSelector) || document.querySelectorAll(elemOrSelector))
        : elemOrSelector;

    // if only one HTMLElement is returned, convert it to an array
    if (elems instanceof HTMLElement) {
        elems = [elems];
    }
    else if (!(elems instanceof Array)) {
        // Convert it to HTMLElement array
        elems = Array.from(elems) as HTMLElement[];
    }

    if (!elems?.length && typeof elemOrSelector === 'string') { elems = document.querySelectorAll(elemOrSelector); }

    // Filter out null or undefined elements
    elems = (elems as HTMLElement[]).filter(elem => elem !== null && elem !== undefined);

    return elems;
}

export function scrollElementIntoView(elemOrSelector: string | HTMLElement) {
    const element = getElement(elemOrSelector);
    element.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
}

export function synchronizeElementScrolling(sourceElemOrSelector: string | HTMLElement, targetElemOrSelector: string | HTMLElement, synchVertical = true, synchHorizontal = false) {
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

export function removeScrollSynchronization(sourceElemOrSelector: string | HTMLElement) {
    const sourceElem = getElement(sourceElemOrSelector);
    if (sourceElem) {
        sourceElem.onscroll = null;
    }
}

export function synchronizeTableRowHover(sourceTable: string | HTMLElement, targetTable: string | HTMLElement) {
    const sourceElem = getElement(sourceTable);
    const targetElem = getElement(targetTable);
    if (targetElem && sourceElem) {
        // forward the mouse events for each row in source table to the corresponding row in target table
        const sourceRows = sourceElem.querySelectorAll('tr');
        const targetRows = targetElem.querySelectorAll('tr');
        sourceRows.forEach((sourceRow, i) => {
            const targetRow = targetRows[i];
            if (targetRow) {
                const mouseOverHandler = (e: MouseEvent) => handleAndCloneMouseEvent(e, sourceRow, targetRow);
                const mouseOutHandler = (e: MouseEvent) => handleAndCloneMouseEvent(e, sourceRow, targetRow);
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

export function removeTableRowHoverSynchronization(sourceTable: string | HTMLElement) {
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

export function synchronizeElementMouseEvents(sourceElemOrSelector: string | HTMLElement, targetElemOrSelector: string | HTMLElement) {
    const sourceElem = getElement(sourceElemOrSelector);
    const targetElem = getElement(targetElemOrSelector);
    if (targetElem && sourceElem) {
        const mouseOverHandler = (e: MouseEvent) => handleAndCloneMouseEvent(e, sourceElem, targetElem);
        const mouseOutHandler = (e: MouseEvent) => handleAndCloneMouseEvent(e, sourceElem, targetElem);

        sourceElem.addEventListener('mouseover', mouseOverHandler);
        sourceElem.addEventListener('mouseout', mouseOutHandler);

        eventHandlersMap.set(sourceElem, {
            mouseover: mouseOverHandler,
            mouseout: mouseOutHandler
        });
    }
}

export function removeScrollMouseEventsSynchronization(sourceElemOrSelector: string | HTMLElement) {
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

export function elementExists(elemOrSelector: string | HTMLElement): boolean {
    return !!getElement(elemOrSelector);
}

function handleAndCloneMouseEvent(event: MouseEvent | ExtendedMouseEvent, elem1: HTMLElement, elem2: HTMLElement) {
    // Prevent infinite loop by checking if the event was already dispatched
    const origEvent = event as ExtendedMouseEvent;
    const originalElement = origEvent?.customDetail?.originalTarget;
    if (originalElement === event.currentTarget) {
        return;
    }

    var curTarget = event.currentTarget as HTMLElement;

    if (event.type === 'mouseover' && !curTarget.classList.contains('force-hover-style')) {
        curTarget.classList.add('force-hover-style');
    }
    else if (event.type === 'mouseout' && curTarget.classList.contains('force-hover-style')) {
        curTarget.classList.remove('force-hover-style');
    }

    const clonedEvent = new MouseEvent(event.type, {
        clientX: event.clientX,
        clientY: event.clientY,
        bubbles: true, // Allow the event to bubble up
        cancelable: true, // Allow the event to be canceled
    }) as ExtendedMouseEvent;

    clonedEvent.customDetail = { originalTarget: origEvent?.customDetail?.originalTarget ?? event.currentTarget };

    event.currentTarget === elem1
        ? elem2.dispatchEvent(clonedEvent)
        : elem1.dispatchEvent(clonedEvent);
}