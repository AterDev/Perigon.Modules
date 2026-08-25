import { Injectable } from '@angular/core';
import { MarkedRenderer } from 'ngx-markdown';
import type { MarkedOptions, Tokens } from 'marked';
import { environment } from 'src/environments/environment';

@Injectable({ providedIn: 'root' })
export class ArticleAssetUrlService {
  private readonly adminOrigin = environment.admin_daemon.replace(/\/+$/, '');

  resolve(path: string): string {
    const storagePath = this.toStoragePath(path);
    return storagePath ? `${this.adminOrigin}${storagePath}` : path;
  }

  toStoragePath(path: string): string | null {
    if (path.startsWith('/article/')) {
      return path;
    }

    try {
      const url = new URL(path, this.adminOrigin);
      const adminUrl = new URL(this.adminOrigin);
      if (url.origin === adminUrl.origin && url.pathname.startsWith('/article/')) {
        return `${url.pathname}${url.search}${url.hash}`;
      }
    } catch {
      return null;
    }

    return null;
  }

  normalizeMarkdown(markdown: string): string {
    const escapedOrigin = this.adminOrigin.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    const imagePattern = new RegExp(
      `(\\!\\[[^\\]]*\\]\\()${escapedOrigin}(\\/article\\/[^)\\s]+)`,
      'g',
    );
    return markdown.replace(imagePattern, '$1$2');
  }

  transformHtml(html: string): string {
    const container = document.createElement('div');
    container.innerHTML = html;
    container.querySelectorAll<HTMLImageElement>('img[src]').forEach((image) => {
      const source = image.getAttribute('src');
      if (source) {
        image.setAttribute('src', this.resolve(source));
      }
    });
    return container.innerHTML;
  }
}

export function articleMarkedOptionsFactory(
  assetUrl: ArticleAssetUrlService,
): MarkedOptions {
  const renderer = new MarkedRenderer();
  const defaultImageRenderer = renderer.image.bind(renderer);
  renderer.image = (token: Tokens.Image) =>
    defaultImageRenderer({ ...token, href: assetUrl.resolve(token.href) });

  return { renderer };
}
