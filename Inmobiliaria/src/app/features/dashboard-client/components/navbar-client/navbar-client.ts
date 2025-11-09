import { Component, HostListener } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
interface SubMenuItem {
  name: string;
  path: string;
  icon: string;
}

interface MenuItem {
  name: string;
  path?: string;
  subItems?: SubMenuItem[];
}

interface MenuSection {
  items: MenuItem[];
}

@Component({
  selector: 'app-navbar-client',
  standalone: false,
  templateUrl: './navbar-client.html',
  styleUrls: ['./navbar-client.css']  // ✅ corregido (plural)
})
export class NavbarClient {

  isMenuOpen: boolean = false;
  isUserMenuOpen: boolean = false;
  openDropdown: string | null = null;
  mobileOpenDropdown: string | null = null;

  userName: string = 'Sandra';
  notificationCount: number = 3;

  menuSections: MenuSection[] = [
    {
      items: [
        { name: 'Dashboard', path: '/dashboard' },
        {
          name: 'Day 1-3',
          subItems: [
            { name: 'Day 1', path: '/day1', icon: 'calendar' },
            { name: 'Day 2', path: '/day2', icon: 'calendar' },
            { name: 'Day 3', path: '/day3', icon: 'calendar' }
          ]
        },
        {
          name: 'Disputed Items',
          subItems: [
            { name: 'Active Disputes', path: '/disputes/active', icon: 'alert-circle' },
            { name: 'Pending', path: '/disputes/pending', icon: 'clock' },
            { name: 'Resolved', path: '/disputes/resolved', icon: 'check-circle' }
          ]
        },
        {
          name: 'Dispute Letters',
          subItems: [
            { name: 'Templates', path: '/letters/templates', icon: 'file-text' },
            { name: 'Sent Letters', path: '/letters/sent', icon: 'send' },
            { name: 'Drafts', path: '/letters/drafts', icon: 'edit' }
          ]
        },
        {
          name: 'Action Plan',
          subItems: [
            { name: 'Current Plan', path: '/action/current', icon: 'list' },
            { name: 'History', path: '/action/history', icon: 'history' }
          ]
        },
        { name: 'Credit Report', path: '/credit-report' },
        {
          name: 'Documents',
          subItems: [
            { name: 'Uploaded', path: '/documents/uploaded', icon: 'upload' },
            { name: 'Generated', path: '/documents/generated', icon: 'file' },
            { name: 'Archive', path: '/documents/archive', icon: 'archive' }
          ]
        }
      ]
    }
  ];
  constructor(private auth: AuthService, private router: Router) { }



  openMenu(menu: string): void {
    this.openDropdown = menu;
  }

  closeMenu(): void {
    this.openDropdown = null;
  }

  toggleMenu(): void {
    this.isMenuOpen = !this.isMenuOpen;
    if (this.isMenuOpen) {
      this.isUserMenuOpen = false;
      this.mobileOpenDropdown = null;
      document.body.style.overflow = 'hidden';
    } else {
      document.body.style.overflow = '';
    }
  }

  toggleDropdown(menu: string): void {
    this.openDropdown = this.openDropdown === menu ? null : menu;
  }

  toggleMobileDropdown(item: string): void {
    this.mobileOpenDropdown = this.mobileOpenDropdown === item ? null : item;
  }

  toggleUserMenu(event: Event): void {
    event.stopPropagation();
    this.isUserMenuOpen = !this.isUserMenuOpen;
    if (this.isUserMenuOpen) {
      this.isMenuOpen = false;
      document.body.style.overflow = '';
    }
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(_: Event): void {
    this.isUserMenuOpen = false;
    this.openDropdown = null;
  }

  @HostListener('window:resize')
  onResize(): void {
    if (window.innerWidth >= 1024) {
      this.isMenuOpen = false;
      this.mobileOpenDropdown = null;
      document.body.style.overflow = '';
    }
  }


  logout(): void {
    this.auth.logout();                  // limpia la sesión (localStorage, etc.)
    this.router.navigate(['/auth/login']); // navega al login
  }
}

