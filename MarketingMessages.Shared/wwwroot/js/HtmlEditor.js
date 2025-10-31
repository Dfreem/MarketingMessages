class HtmlEditor {
    constructor(options) {
        this.options = options;
        this.id = options.id;
        this.images = new Map();
        this.selectedImage = null;
        this.dirty = false;

        this.handleClick = this.handleClick.bind(this);
        this.replaceImgSrc = this.replaceImgSrc.bind(this);
        this.uploadImage = this.uploadImage.bind(this);
        this.getContent = this.getContent.bind(this);
        this.addRow = this.addRow.bind(this);
        this.handleFocus = this.handleFocus.bind(this);
        this.handleBlur = this.handleBlur.bind(this);
        this.handleKeyDown = this.handleKeyDown.bind(this);
        this.handleMutations = this.handleMutations.bind(this);
        this.placeCaretAtStart = this.placeCaretAtStart.bind(this);
        this.resetContent = this.resetContent.bind(this);
        this.didSave = this.didSave.bind(this);

        this._DOM = document.getElementById(options.id);

        // check for existing rows, the editor may have existing content
        let rows = document.querySelectorAll('._row');
        this.rows = rows ? Array.from(rows) : [];

        new Promise(res => {
            res();
            this._DOM.addEventListener('click', this.handleClick);
            this._DOM.addEventListener('focus', this.handleFocus);
            const config = { attributes: true, childList: true, subtree: true };
            const observer = new MutationObserver(this.handleMutations);
            observer.observe(this._DOM, config);
        });

    }
    didSave() {
        this.dirty = false;
        window.sessionStorage.removeItem('inProgress');
    }
    execCommand(command, value) {
        this.dirty = true;
        document.execCommand(command, false, value || null);
    }
    handleClipboardPaste(clipboardEvent) {
        console.log(clipboardEvent);
    }
    formatBlock(blockType) {
        this.dirty = true;
        document.execCommand("formatBlock", false, blockType);
    }
    getContent() {
        return this._DOM.innerHTML;
    }
    clearContent() {
        this._DOM.innerHTML = '';
        this.images.clear();
        this.rows = [];
    }
    resetContent(html) {
        if (!this._DOM) {
            this._DOM = document.getElementById(this.options.id);
        }
        this._DOM.innerHTML = html;

        let rows = this._DOM.querySelectorAll('._row');
        this.rows = rows ? Array.from(rows) : [];

        this.images.clear();
        window.sessionStorage.removeItem('inProgress');
        this.dirty = false;
    }
    uploadImage() {
        const input = document.createElement('input');
        input.setAttribute('type', 'file');
        input.setAttribute('accept', 'image/*');
        input.click();

        input.addEventListener('change', (e) => {
            const loading = new bootstrap.Modal(document.getElementById('loading-modal'));
            loading.show();
            let row = this.addRow();
            this._DOM.append(row);
            const file = e.target.files[0];
            const reader = new FileReader();
            reader.addEventListener('loadend', (e) => {
                loading.hide();
            })
            reader.addEventListener('load', (e) => {
                let base64 = e.target.result;
                let imageId = file.name || 'temp-img-id';
                let img = new EditorImage({ src: base64, id: imageId, parent: row });
                this.images.set(imageId, img);
                this.options.dotnetRef.invokeMethodAsync(this.options.imageCallback, imageId, file.name, base64);
                this.dirty = true;
            });
            reader.readAsDataURL(file);
        });
    }
    replaceImgSrc(id, src) {
        let image = this.images.get(`${id}`).element;
        if (!image) {
            image = this.images.get('temp-img-id').element;
            image.setAttribute('id', id);
        }
        if (!image) {
            console.warn("unable to find image element");
            return "unable to find image element";
        }
        image.setAttribute('src', src);
        return this._DOM.innerHTML;
    }
    handleMutations(mutations, obs) {
        // track change state
        if (this._DOM.innerHTML !== '') this.dirty = true;

        for (const mutation of mutations) {
            if (mutation.removedNodes?.length != 0) {
                mutation.removedNodes.forEach((node) => {
                    if (node.nodeName === "IMG" || node.classList?.contains('image-wrapper')) {
                        let img = this.images.get(`${node.id}`);
                        this.images.delete(img);
                    }
                })
            }
            if (mutation.addedNodes?.length != 0) {
                mutation.addedNodes.forEach((node) => {
                    console.log("added node", node);
                    if (node.nodeName === "IMG" && !this.images.get(`${node.id}`)) {
                        let editorImg = new EditorImage({ imageNode: node, id: node.id });
                        this.images.set(node.id, editorImg)
                        this._DOM.append(editorImg.wrapper);
                    }
                })
            }
        }
        obs.takeRecords();

    }
    handleClick(e) {
        // clicking an image or it's wrapper will attach a resize handle and a position toolbar
        if (e.target.tagName === 'IMG' || e.target.classList.contains('image-wrapper')) {
            let image = e.target.tagName === 'IMG' ? e.target : e.target.querySelector('img');
            let id = image.getAttribute('id');
            this.selectedImage = this.images.get(`${id}`);
            if (!this.selectedImage) {
                let parent = image.parentElement;
                let editorImage = new EditorImage({ imageNode: image })
                if (parent && !parent.classList.contains('image-wrapper')) {
                    parent.append(editorImage.wrapper);
                }
                else {
                    this._DOM.append(editorImage.wrapper);
                }
                this.images.set(id, editorImage);
                this.selectedImage = editorImage;
            }
            this.selectedImage.createResizer();
        }
        else {
            this.images.forEach((i) => {
                i.hideResizer();
            })
            const rect = this._DOM.getBoundingClientRect();
            const clickX = e.clientX - rect.left;

            // Get first child (might be image or text)
            const firstChild = e.target.firstChild;

            if (firstChild && firstChild.nodeType === 1 && (firstChild.classList.contains('image-wrapper'))) {
                // If the click is in the left-hand whitespace
                if (clickX < firstChild.getBoundingClientRect().left - rect.left) {
                    // Ensure there is a paragraph at the top
                    if (!this._DOM.firstElementChild || this._DOM.firstElementChild.tagName !== "P") {
                        const p = document.createElement("p");
                        const inner = document.createElement("p");
                        p.append(inner);
                        p.innerHTML = "<br>"; // so caret can land
                        firstChild.before(p);
                    }

                    this.placeCaretAtStart(this._DOM.firstElementChild);
                }
            }
        }
    }
    handleFocus(e) {
        document.addEventListener('keydown', this.handleKeyDown);
    }
    handleBlur(e) {
        document.removeEventListener('keydown', this.handleKeyDown);
        this.images.forEach((img) => {
            img.hideResizer();
            img.hideToolbar();
        });
    }
    handleKeyDown(keyboardEvent) {
        if (keyboardEvent.key === 'Enter') {
            let selection = document.getSelection();
            let newLine = document.createElement('br');
            selection.anchorNode.after(newLine);
            this.placeCaretAtStart(newLine);
        }
    }
    placeCaretAtStart(el) {
        const range = document.createRange();
        const sel = window.getSelection();
        range.setStart(el, 0);
        range.collapse(true);
        sel.removeAllRanges();
        sel.addRange(range);
    }
    addRow() {
        let row = document.createElement('div');
        row.style.display = 'flex';
        row.style.width = '100%';
        row.classList.add('_row');
        $(this._DOM).append(row);
        this.rows.push(row);
        return row;
    }
    removeRow(index) {
        if (index) {
            let row = this.rows.splice(index, 1);
            row.remove();
        }
    }
}

class EditorImage {
    constructor(args) {
        const src = args.src;
        const id = args.id | `${Date.now()}`;
        this.id = id;
        const parent = args.parent;
        const imageNode = args.imageNode;

        // a src url was found in args
        if (src) {
            this.element = document.createElement('img');
            this.element.setAttribute('src', src);
            this.element.setAttribute('width', 400);
        }
        // an img element was found in args
        else if (imageNode) {
            this.element = imageNode;
        }
        // 
        if (id) {
            this.element.setAttribute('id', id);
        }
        if (this.element.parentElement.classList.contains('image-wrapper')) {
            this.wrapper = this.element.parentElement;
        }
        else {

            this.wrapper = document.createElement('div');
            this.wrapper.classList.add('image-wrapper');
            this.wrapper.append(this.element);
        }
        this.wrapper.style.width = 'fit-content';
        this.wrapper.style.height = 'fit-content';
        this.wrapper.style.position = 'relative';
        this.wrapper.style.resize = 'both';
        this.wrapper.style.contenteditable = false;
        this.element.style.contenteditable = false;
        this.wrapper.style.display = 'block';
        this.wrapper.style.cursor = 'move';

        //if the image is a row element or some other wrapper besides image-wrapper 
        // we want to add this to that parent
        if (typeof parent !== 'undefined') {
            if (!parent.classList.contains('image-wrapper')) {
                parent.append(this.wrapper);
            }
        }

        this.resizer = null;
        this.hideResizer = this.hideResizer.bind(this);
        this.createResizer = this.createResizer.bind(this);
        this.setSrc = this.setSrc.bind(this);
        this.isDragging = false;
        this.offsetX = 0;
        this.offsetY = 0;
        this.wrapper.addEventListener('mousedown', e => {
            e.preventDefault();
            this.isDragging = true;

            // Get current offsets from computed style
            const computedStyle = window.getComputedStyle(this.wrapper);
            const currentLeft = parseInt(computedStyle.left) || 0;
            const currentTop = parseInt(computedStyle.top) || 0;

            // Store the mouse start position
            this.startX = e.clientX;
            this.startY = e.clientY;

            const mouseMove = (e) => {
                if (!this.isDragging) return;

                const dx = e.clientX - this.startX;
                const dy = e.clientY - this.startY;

                // Apply relative offsets correctly
                this.wrapper.style.left = `${currentLeft + dx}px`;
                this.wrapper.style.top = `${currentTop + dy}px`;
            };

            const mouseUp = (e) => {
                e.preventDefault();
                this.isDragging = false;
                document.removeEventListener('mousemove', mouseMove);
                document.removeEventListener('mouseup', mouseUp);
            };

            document.addEventListener('mousemove', mouseMove);
            document.addEventListener('mouseup', mouseUp);
        });

    }
    hideToolbar() {
        if (!this.toolbar) return;
        this.toolbar.remove();
        this.toolbar = null;
    }
    hideResizer() {
        if (!this.resizer) return;
        this.resizer.remove();
        this.resizer = null;
    }
    //hideDragHandle() {
    //    if (!this.dragHandle) return;
    //    this.dragHandle.remove();
    //    this.dragHandle = null;
    //}
    setSrc(src) {
        this.element.setAttribute('src', src);
    }
    createResizer() {
        if (this.resizer) {
            return;
        }
        this.resizer = document.createElement('div');
        this.resizer.classList.add('image-resizer');
        this.resizer.style.left = window.getComputedStyle(this.wrapper).getPropertyValue('width');
        this.resizer.style.width = '15px';
        this.resizer.style.height = '15px';
        this.resizer.style.background = '#aaa';
        this.resizer.style.cursor = 'nwse-resize';
        this.resizer.style.marginInlineStart = 'auto';
        this.resizer.addEventListener('mousedown', (e) => {
            e.preventDefault();
            if (!this.element) {
                return;
            }
            let startX = e.clientX;
            let startWidth = this.element.offsetWidth;

            const doDrag = ev => {
                ev.preventDefault();
                let newWidth = startWidth + (ev.clientX - startX);
                this.element.style.width = newWidth + 'px';
                this.wrapper.style.width = newWidth + 'px';
            };

            const stopDrag = (ev) => {
                ev.preventDefault();
                ev.stopPropagation();
                document.removeEventListener('mousemove', doDrag);
                document.removeEventListener('mouseup', stopDrag);
            };

            document.addEventListener('mousemove', doDrag);
            document.addEventListener('mouseup', stopDrag);
        });
        this.wrapper.append(this.resizer);
    }
}

export function createEditor(options) {
    const editor = new HtmlEditor(options);
    let inProgress = window.sessionStorage.getItem('inProgress');
    if (inProgress) {
        editor.resetContent(inProgress);
    }
    window.wysiwyg = editor;
}

export function disposeEditor() {
    if (window.wysiwyg) {

        if (window.wysiwyg.dirty) {
            window.sessionStorage.setItem('inProgress', window.wysiwyg._DOM.innerHTML);
        }
        window.wysiwyg._DOM.innerHtml = '';
        window.wysiwyg._DOM = null;
        window.wysiwyg = null;
    }
}

