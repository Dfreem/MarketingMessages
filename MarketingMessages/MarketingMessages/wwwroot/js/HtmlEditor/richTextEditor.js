//const Parchment = Quill.import('parchment');
//const StyleAttributor = Parchment.StyleAttributor;
//const BlockEmbed = Quill.import('blots/block/embed');
//const InlineEmbed = Quill.import('blots/embed');
//const editorState = { selector: '#editor' }

//const AlignStyle = Quill.import('attributors/style/align');
//Quill.register(AlignStyle, true);

//const BackgroundStyle = Quill.import('attributors/style/background');
//Quill.register(BackgroundStyle, true);

//const ColorStyle = Quill.import('attributors/style/color');
//Quill.register(ColorStyle, true);

//const DirectionStyle = Quill.import('attributors/style/direction');
//Quill.register(DirectionStyle, true);

//const FontStyle = Quill.import('attributors/style/font');
//Quill.register(FontStyle, true);

//const SizeStyle = Quill.import('attributors/style/size');
//Quill.register(SizeStyle, true);

//const FloatStyle = new StyleAttributor('float', 'float', {
//    whitelist: ['left', 'right', 'none', 'inherit']
//});
//Quill.register(FloatStyle, true);

///**
// * Initialize Quill as a rich text editor. Provide callbacks to register with the editor and it's modules.
// * @param {Object} dotnetRef
// * @param {string[]} matchTags
// * @param {Function} callback
// * @param {Function} matchTagsCallback
// * @param {Function} readyCallback
// * @param {Function} imageUploadCallback
// */
//export function initEditor(dotnetRef, matchTags, callback, matchTagsCallback, readyCallback, imageUploadCallback) {

//    Quill.register('blots/block/embed', ImageBlot);
//    Quill.register('modules/tag', TagsMatcher);
//    //Quill.register('modules/imageToolbar', ImageToolbar);

//    const toolbarOptions = [
//        ['bold', 'italic', 'underline', 'strike'],        // toggled buttons
//        ['blockquote', 'code-block'],
//        ['link', 'image', 'video', 'formula'],

//        [{ 'header': 1 }, { 'header': 2 }],               // custom button values
//        [{ 'list': 'ordered' }, { 'list': 'bullet' }, { 'list': 'check' }],
//        [{ 'script': 'sub' }, { 'script': 'super' }],      // superscript/subscript
//        [{ 'indent': '-1' }, { 'indent': '+1' }],          // outdent/indent
//        [{ 'direction': 'rtl' }],                         // text direction

//        [{ 'size': ['small', false, 'large', 'huge'] }],  // custom dropdown
//        [{ 'header': [1, 2, 3, 4, 5, 6, false] }],

//        [{ 'color': [] }, { 'background': [] }],          // dropdown with defaults from theme
//        [{ 'font': [] }],
//        [{ 'align': [] }],

//        ['clean']                                         // remove formatting button
//    ];

//    const quill = new Quill('#editor', {
//        modules: {
//            toolbar: {
//                options: {
//                    debug: 'info'
//                },

//                container: toolbarOptions,
//                handlers: {
//                    'code-block': () => {
//                        let modal = document.querySelector('#code-editor');
//                        modal = bootstrap.Modal.getOrCreateInstance(modal);
//                        modal.show();
//                    },
//                    image: () => {
//                        const input = document.createElement('input');
//                        input.setAttribute('type', 'file');
//                        input.setAttribute('accept', 'image/*');
//                        input.click();

//                        input.onchange = () => {
//                            const file = input.files[0];
//                            const reader = new FileReader();
//                            reader.onload = (e) => {
//                                const base64 = e.target.result;
//                                const range = quill.getSelection();
//                                quill.insertEmbed(range.index, 'image', base64, 'user');
//                                let textContent = quill.getText();
//                                let htmlContent = quill.getSemanticHTML();

//                                // Call .NET only once, directly
//                                dotnetRef.invokeMethodAsync(imageUploadCallback, file.name, base64);
//                                dotnetRef.invokeMethodAsync(callback, htmlContent, textContent);
//                            };
//                            reader.readAsDataURL(file);
//                        };
//                    }
//                }
//            },
//            tag: {
//                words: matchTags,
//                onMatch: function (word) {
//                    console.log(word);
//                    dotnetRef.invokeMethodAsync(matchTagsCallback, word);
//                }
//            }
//        },
//        theme: 'snow'
//    });

//    quill.on("selection-change", async (range, oldRange, src) => {
//        if (range == null) {
//            hideResizers();
//        }
//        if (quill.hasFocus()) return;
//        const html = quill.getSemanticHTML(); // lighter than getSemanticHTML
//        const text = quill.getText();
//        await dotnetRef.invokeMethodAsync(callback, html, text);
//    });

//    quill.root.addEventListener('click', e => {
//        e.stopPropagation();
//        let img = e.target.closest('img');
//        let wrapper = e.target.closest('.ql-image-wrapper');
//        hideResizers();
//        if (wrapper && img) {
//            ImageBlot.createResizer(wrapper);
//            ImageBlot.createToolbar(wrapper);

//        }
//    });
//    quill.clipboard.addMatcher('img', function (node, delta) {
//        const src = node.getAttribute('src');
//        const alt = node.getAttribute('alt') || '';
//        const style = node.getAttribute('style') || node.parentElement.getAttribute('style');
//        const id = node.id || 'temp-image-id';
//        if (src.includes('base64')) {
//            dotnetRef.invokeMethodAsync(imageUploadCallback, id, src);
//        }

//        // You can store extra metadata on the blot if you want
//        delta.ops = [{
//            insert: {
//                image: { src: src, alt: alt, style: style, id: id }
//            }
//        }];
//        return delta;
//    });

//    function hideResizers() {
//        let images = document.querySelectorAll('.ql-image-wrapper');
//        if (images) {
//            Array.from(images).map(i => {
//                let resizer = i.parentElement.querySelector('.ql-image-resizer');
//                let toolbar = i.parentElement.querySelector('.ql-image-toolbar');
//                if (resizer) {
//                    resizer.remove();
//                }
//                if (toolbar) {
//                    toolbar.remove();
//                }
//            })
//        }
//    }
//    dotnetRef.invokeMethodAsync(readyCallback);
//}

//// ------------------- Module Functions -------------------
//export function resetEditorContent(content) {
//    let editorElement = document.querySelector(editorState.selector);
//    let editor = Quill.find(editorElement);
//    editor.setContents([]);
//    editor.clipboard.dangerouslyPasteHTML(0, content, 'silent');
//    editor.update();
//    //editor.enable(true);
//}

//export function replaceImgSrc(id, src) {
//    let image = document.querySelector(`#${id}`);
//    if (!image) {
//        image = document.querySelector('#temp-img-id');
//        image.id = id;
//    }
//    if (!image) {
//        console.warn("unable to find image element");
//        return;
//    }

//    let editor = Quill.find(document.querySelector(editorState.selector));
//    image.setAttribute('src', src);
//    editor.update()
//    return editor.getSemanticHTML();;
//}

//export function getEditorContent() {
//    let editor = document.querySelector(editorState.selector);
//    editor = Quill.find(editor);
//    return editor.getSemanticHTML();
//}

//// --------------- Modules ---------------

//class TagsMatcher {
//    constructor(quill, options) {
//        this.quill = quill;
//        this.options = options || {};
//        this.onMatch = typeof options.onMatch === 'function' ? options.onMatch : null;

//        // Normalize words list (optional case-insensitivity)
//        const caseSensitive = !!options.caseSensitive;
//        this.normalize = w => (caseSensitive ? w : String(w).toLowerCase());
//        this.wordSet = new Set((options.words || []).map(this.normalize));

//        this._debounceId = null;
//        this._debounce = (fn, ms = 80) => {
//            clearTimeout(this._debounceId);
//            this._debounceId = setTimeout(fn, ms);
//        };

//        // Run after any user-initiated change; paste/drag also come through as 'user'
//        quill.on('text-change', (_delta, _old, source) => {
//            if (source !== 'user') return;
//            this._debounce(() => this.scanAll());
//        });

//        // Extra safety: run after editor-change as well (cursor settles after paste/drop)
//        quill.on('editor-change', () => {
//            this._debounce(() => this.scanAll());
//        });

//        // Belt & suspenders: react to native paste/drop on root
//        quill.root.addEventListener('paste', () => this._debounce(() => this.scanAll()));
//        quill.root.addEventListener('drop', () => this._debounce(() => this.scanAll()));
//    }

//    scanAll() {
//        if (!this.onMatch || this.wordSet.size === 0) return;

//        // Plain text view (images/embeds are ignored), cheap even for big docs
//        const text = this.quill.getText();   // includes trailing '\n'
//        if (!text) return;

//        // Tokenize by non-whitespace (keeps hashtags/@tags intact if you use them)
//        const words = text.match(/\S+/g) || [];

//        // Dedupe so you don’t get spammed for repeated words unless you want that
//        const dedupe = this.options.dedupe !== false;
//        if (dedupe) {
//            const seen = new Set();
//            for (let i = 0; i < words.length; i++) {
//                const w = this.normalize(words[i]);
//                if (seen.has(w)) continue;
//                if (this.wordSet.has(w)) {
//                    seen.add(w);
//                    this.onMatch(words[i]); // pass original casing
//                }
//            }
//        } else {
//            // Fire for every occurrence
//            for (let i = 0; i < words.length; i++) {
//                if (this.wordSet.has(this.normalize(words[i]))) {
//                    this.onMatch(words[i]);
//                }
//            }
//        }
//    }
//}

//// ----------- Parchement Extensions -----------------

//class ImageBlot extends BlockEmbed {
//    static blotName = 'image';
//    static tagName = 'img';

//    static initNode(node) {
//        node.setAttribute('contenteditable', false);
//        node.style.maxWidth = '100%';
//        node.style.height = 'auto';
//        node.draggable = true;
//    }

//    static createResizer(wrapper) {

//        let resizer = document.createElement('div');
//        resizer.classList.add('ql-image-resizer');
//        resizer.style.left = window.getComputedStyle(wrapper).getPropertyValue('width');
//        resizer.style.width = '15px';
//        resizer.style.height = '15px';
//        resizer.style.background = 'rgba(0,0,0,0.5)';
//        resizer.style.cursor = 'nwse-resize';
//        resizer.style.marginInlineStart = 'auto';
//        // Handle resizing
//        //wrapper.appendChild(resizer);
//        resizer.addEventListener('mousedown', e => {
//            let img = e.target.parentElement.querySelector('img');
//            if (!img) {
//                return;
//            }
//            let startX = e.clientX;
//            let startWidth = img.offsetWidth;

//            const doDrag = ev => {
//                let newWidth = startWidth + (ev.clientX - startX);
//                img.style.width = newWidth + 'px';
//                e.target.parentElement.style.width = newWidth + 'px';
//            };

//            const stopDrag = (ev) => {
//                document.removeEventListener('mousemove', doDrag);
//                document.removeEventListener('mouseup', stopDrag);
//            };

//            document.addEventListener('mousemove', doDrag);
//            document.addEventListener('mouseup', stopDrag);
//        });
//        wrapper.appendChild(resizer);
//    }
//    static createToolbar(wrapper) {
//        let existing = document.querySelectorAll('.ql-image-toolbar')
//        if (existing) {
//            Array.from(existing).map(e => {
//                e.remove();
//            })
//        }
//        let container = document.createElement('div');
//        container.classList.add('ql-image-toolbar');
//        container.style.position = 'absolute';
//        container.style.bottom = '0';
//        container.style.width = 'fit-content';
//        container.style.display = 'flex';
//        container.style.background = '#fff';
//        container.style.border = '1px solid #ccc';
//        container.style.padding = '4px';
//        container.style.borderRadius = '6px';
//        container.style.boxShadow = '0 2px 6px rgba(0,0,0,0.2)';

//        const buttons = [
//            { name: 'left', iconClass: 'bi bi-chevron-left' },
//            { name: 'center', iconClass: 'bi bi-chevron-up' },
//            { name: 'right', iconClass: 'bi bi-chevron-right' },
//            { name: 'delete', iconClass: 'bi bi-x' }
//        ];

//        buttons.forEach(b => {
//            let btn = document.createElement('button');
//            btn.setAttribute('name', b.name);
//            let btnIcon = document.createElement('i');
//            btn.classList.add('btn');
//            if (b.name === 'delete') {
//                btn.classList.add('btn-danger', 'text-bg-danger');
//            } else {
//                btn.classList.add('btn-200');
//            }
//            btnIcon.classList = b.iconClass;
//            btn.appendChild(btnIcon);
//            btn.addEventListener('click', e => {
//                e.stopPropagation();
//                e.preventDefault();
//                let img = wrapper.querySelector('img');
//                let target = e.target.closest('button');
//                if (target == null || img == null) return;
//                switch (target.name) {
//                    case 'left':
//                        wrapper.style.float = 'left';
//                        wrapper.style.margin = '0 1em 1em 0';
//                        break;
//                    case 'center':
//                        wrapper.style.float = 'unset';
//                        wrapper.style.marginInline = 'auto';
//                        wrapper.style.width = 'fit-content';
//                        break;
//                    case 'right':
//                        wrapper.style.float = 'right';
//                        wrapper.style.margin = '0 0 1em 1em';
//                        break;
//                    case 'delete':
//                        wrapper.remove();
//                        //document.body.removeChild(this.toolbar);
//                        //this.quill.root.removeChild(this.imageWrapper);
//                        //this.toolbar = null;
//                        //this.imageWrapper = null;
//                        //this.currentImage = null;
//                        break;
//                }
//            });
//            container.appendChild(btn);
//        });

//        wrapper.appendChild(container);

//    }

//    static create(value) {
//        const image = super.create();
//        const node = document.createElement('div');
//        if (typeof value === 'string') {
//            image.setAttribute('src', value);
//        } else if (typeof value === 'object') {
//            if (value.src) image.setAttribute('src', value.src);
//            if (value.alt) image.setAttribute('alt', value.alt);
//            if (value.style) image.setAttribute('style', value.style);
//            if (value.id) image.id = value.id;
//        }
//        node.style.width = image.style.width || 'fit-content';
//        node.style.float = image.style.float;

//        node.classList.add('ql-image-wrapper');
//        node.style.height = 'fit-content';
//        node.style.display = 'block';

//        ImageBlot.initNode(node);

//        node.appendChild(image);

//        //ImageBlot.createResizer(node);

//        //ImageBlot.createToolbar(node);

//        // click & drag events…

//        return node;
//    }

//    static formats(node) {
//        return ImageBlot.value(node);
//    }
//    static value(node) {
//        let img = node.closest('img');
//        if (!img) img = node.querySelector('img');
//        return {
//            src: img?.getAttribute('src') || '',
//            alt: img?.getAttribute('alt') || '',
//            style: img?.getAttribute('style') || '',
//            id: img?.id || 'temp-img-id'
//        };
//    }

//}

