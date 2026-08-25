import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  ElementRef,
  EventEmitter,
  NgZone,
  OnDestroy,
  AfterViewInit,
  Output,
  ViewChild,
  forwardRef,
  inject,
} from '@angular/core';
import {
  ControlValueAccessor,
  NG_VALUE_ACCESSOR,
} from '@angular/forms';
import Vditor from 'vditor';
import { ArticleAssetUrlService } from '../../services/article-asset-url.service';
import { environment } from 'src/environments/environment';

const MAX_IMAGE_SIZE = 10 * 1024 * 1024;
const IMAGE_TYPES = new Set([
  'image/png',
  'image/jpeg',
  'image/gif',
  'image/webp',
]);
const VDITOR_CDN = '/assets/vditor';
const VDITOR_CONTENT_THEME_PATH = `${VDITOR_CDN}/dist/css/content-theme`;
const EDITOR_TOOLBAR = [
  'headings',
  'bold',
  'italic',
  'strike',
  'link',
  '|',
  'quote',
  'list',
  'ordered-list',
  'check',
  '|',
  'code',
  'inline-code',
  'table',
  'upload',
  '|',
  'undo',
  'redo',
  '|',
  'fullscreen',
  'edit-mode',
  {
    name: 'more',
    toolbar: ['both', 'code-theme', 'content-theme', 'outline', 'preview', 'help'],
  },
];

@Component({
  selector: 'app-markdown-editor',
  standalone: true,
  template: '<div #editorHost class="markdown-editor-host"></div>',
  styleUrl: './markdown-editor.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => MarkdownEditorComponent),
      multi: true,
    },
  ],
})
export class MarkdownEditorComponent
  implements ControlValueAccessor, AfterViewInit, OnDestroy
{
  @ViewChild('editorHost', { static: true })
  private readonly editorHost!: ElementRef<HTMLDivElement>;

  @Output() readonly markdownChange = new EventEmitter<string>();
  @Output() readonly uploadStart = new EventEmitter<void>();
  @Output() readonly uploadSuccess = new EventEmitter<string>();
  @Output() readonly uploadError = new EventEmitter<string>();

  private readonly zone = inject(NgZone);
  private readonly changeDetector = inject(ChangeDetectorRef);
  private readonly assetUrl = inject(ArticleAssetUrlService);
  private editor?: Vditor;
  private imageObserver?: MutationObserver;
  private colorScheme?: MediaQueryList;
  private readonly colorSchemeChange = (event: MediaQueryListEvent): void => {
    this.setTheme(event.matches);
  };
  private value = '';
  private editorReady = false;
  private destroyed = false;
  private disabled = false;
  private onChange: (value: string) => void = () => undefined;
  private onTouched: () => void = () => undefined;

  ngAfterViewInit(): void {
    this.colorScheme = this.getColorScheme();
    this.colorScheme?.addEventListener('change', this.colorSchemeChange);
    if (typeof MutationObserver !== 'undefined') {
      this.imageObserver = new MutationObserver(() => this.normalizeEditorImages());
      this.imageObserver.observe(this.editorHost.nativeElement, {
        childList: true,
        subtree: true,
      });
    }

    this.editor = new Vditor(this.editorHost.nativeElement, {
      value: this.value,
      mode: 'sv',
      cdn: VDITOR_CDN,
      theme: this.colorScheme?.matches ? 'dark' : 'classic',
      toolbar: EDITOR_TOOLBAR,
      height: 620,
      minHeight: 420,
      lang: 'zh_CN',
      counter: { enable: true, max: 200000 },
      cache: { enable: false },
      preview: {
        mode: 'both',
        maxWidth: 900,
        theme: {
          current: this.colorScheme?.matches ? 'dark' : 'light',
          list: { light: '浅色', dark: '深色' },
          path: VDITOR_CONTENT_THEME_PATH,
        },
        hljs: {
          style: this.colorScheme?.matches ? 'github-dark' : 'github',
        },
        transform: (html) => this.assetUrl.transformHtml(html),
      },
      upload: {
        url: `${environment.admin_daemon.replace(/\/+$/, '')}/api/Article/images`,
        max: MAX_IMAGE_SIZE,
        accept: 'image/png,image/jpeg,image/gif,image/webp',
        multiple: false,
        fieldName: 'file',
        setHeaders: () => this.getUploadHeaders(),
        validate: (files) => this.validateUpload(files),
        format: (files, responseText) =>
          this.formatUploadResponse(files, responseText),
        error: (message) => this.emitUploadError(message),
      },
      input: (markdown) => this.emitValue(markdown),
      blur: () => this.zone.run(() => this.onTouched()),
      after: () => {
        if (this.destroyed) return;
        this.editorReady = true;
        if (this.editor?.getValue() !== this.value) {
          this.editor?.setValue(this.value, true);
        }
        this.normalizeEditorImages();
      },
    });

    if (this.disabled) {
      this.editor.disabled();
    }
  }

  writeValue(value: string | null): void {
    this.value = value ?? '';
    if (this.editorReady) {
      this.editor?.setValue(this.value, true);
    }
  }

  registerOnChange(onChange: (value: string) => void): void {
    this.onChange = onChange;
  }

  registerOnTouched(onTouched: () => void): void {
    this.onTouched = onTouched;
  }

  setDisabledState(disabled: boolean): void {
    this.disabled = disabled;
    if (disabled) {
      this.editor?.disabled();
    } else {
      this.editor?.enable();
    }
    this.changeDetector.markForCheck();
  }

  ngOnDestroy(): void {
    this.destroyed = true;
    this.imageObserver?.disconnect();
    this.colorScheme?.removeEventListener('change', this.colorSchemeChange);
    this.editor?.destroy();
  }

  private emitValue(markdown: string): void {
    this.value = this.assetUrl.normalizeMarkdown(markdown);
    this.zone.run(() => {
      this.onChange(this.value);
      this.markdownChange.emit(this.value);
    });
  }

  private normalizeEditorImages(): void {
    this.editorHost.nativeElement
      .querySelectorAll<HTMLImageElement>('img[src]')
      .forEach((image) => {
        const source = image.getAttribute('src');
        if (!source) return;

        const storagePath = this.assetUrl.toStoragePath(source);
        if (!storagePath) return;

        image.dataset['articleAssetPath'] = storagePath;
        const resolvedPath = this.assetUrl.resolve(storagePath);
        if (source !== resolvedPath) {
          image.setAttribute('src', resolvedPath);
        }
      });
  }

  private getColorScheme(): MediaQueryList | undefined {
    if (typeof window === 'undefined' || !window.matchMedia) return undefined;
    return window.matchMedia('(prefers-color-scheme: dark)');
  }

  private setTheme(isDark: boolean): void {
    const theme = isDark ? 'dark' : 'classic';
    const contentTheme = isDark ? 'dark' : 'light';
    const codeTheme = isDark ? 'github-dark' : 'github';
    this.editor?.setTheme(theme, contentTheme, codeTheme, VDITOR_CONTENT_THEME_PATH);
  }

  private validateUpload(files: File[]): string | boolean {
    const file = files[0];
    if (!file) {
      return '请选择图片';
    }
    if (file.size > MAX_IMAGE_SIZE) {
      return '图片不能超过10MB';
    }
    if (!IMAGE_TYPES.has(file.type)) {
      return '仅支持 PNG、JPEG、GIF、WebP 图片';
    }

    this.zone.run(() => this.uploadStart.emit());
    return true;
  }

  private formatUploadResponse(files: File[], responseText: string): string {
    try {
      const result = JSON.parse(responseText) as { path?: string };
      if (!result.path?.startsWith('/article/')) {
        throw new Error('上传接口返回的图片路径无效');
      }

      this.zone.run(() => this.uploadSuccess.emit(result.path));
      return JSON.stringify({
        msg: '',
        code: 0,
        data: {
          errFiles: [],
          succMap: { [files[0]?.name ?? 'image']: result.path },
        },
      });
    } catch (error) {
      const message = error instanceof Error ? error.message : '图片上传失败';
      this.emitUploadError(message);
      return JSON.stringify({ msg: message, code: 1 });
    }
  }

  private emitUploadError(message: string): void {
    const displayMessage = this.getUploadErrorMessage(message);
    this.zone.run(() => {
      this.editor?.tip(displayMessage, 5000);
      this.uploadError.emit(displayMessage);
    });
  }

  private getUploadErrorMessage(message: string): string {
    if (!message) {
      return '图片上传失败';
    }

    try {
      const response = JSON.parse(message) as {
        detail?: string;
        title?: string;
      };
      return response.detail ?? response.title ?? message;
    } catch {
      return message;
    }
  }

  private getUploadHeaders(): Record<string, string> {
    const token = localStorage.getItem('accessToken');
    return token ? { Authorization: `Bearer ${token}` } : {};
  }
}
