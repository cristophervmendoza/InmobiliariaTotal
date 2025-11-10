import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';

// Tipado mínimo de grecaptcha
declare global {
  interface Window { grecaptcha: any; }
}

export interface CaptchaValidateResponse { ok: boolean; }

@Injectable({ providedIn: 'root' })
export class CaptchaService {
  private siteKey = environment.recaptchaSiteKey;
  private apiBase = environment.apiBaseUrl;
  private scriptLoaded = false;
  private loadingPromise?: Promise<void>;

  constructor(private http: HttpClient) { }

  private loadScript(): Promise<void> {
    if (this.scriptLoaded) return Promise.resolve();
    if (this.loadingPromise) return this.loadingPromise;

    this.loadingPromise = new Promise<void>((resolve, reject) => {
      const s = document.createElement('script');
      s.src = `https://www.google.com/recaptcha/api.js?render=${this.siteKey}`;
      s.async = true; s.defer = true;
      s.onload = () => { this.scriptLoaded = true; resolve(); };
      s.onerror = () => reject(new Error('No se pudo cargar reCAPTCHA v3'));
      document.head.appendChild(s);
    });
    return this.loadingPromise;
  }

  async execute(action: string): Promise<string> {
    if (!this.siteKey) throw new Error('SiteKey no configurada');
    await this.loadScript();

    return new Promise<string>((resolve, reject) => {
      if (!window.grecaptcha?.ready) return reject(new Error('reCAPTCHA no inicializado'));
      window.grecaptcha.ready(() => {
        window.grecaptcha.execute(this.siteKey, { action })
          .then((token: string) => resolve(token))
          .catch((err: any) => reject(err));
      });
    });
  }

  validateOnServer(token: string, action: string) {
    return this.http.post<CaptchaValidateResponse>(`${this.apiBase}/api/Captcha/validate`, { token, action });
  }
}
