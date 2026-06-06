import { ThemeToggler } from "@/utils/ThemeToggler";
import { SidebarMenu, SidebarMenuItem, useSidebar } from "./ui/sidebar";

export default function NavUtils() {
  const { isMobile } = useSidebar();
  return (
    <SidebarMenu>
      <SidebarMenuItem>
        <div className="peer/menu-button group/menu-button flex w-full items-center gap-2 overflow-hidden rounded-md p-2 text-left ring-sidebar-ring outline-hidden transition-[width,height,padding] group-has-data-[sidebar=menu-action]/menu-item:pr-8 group-data-[collapsible=icon]:size-8! focus-visible:ring-2 disabled:pointer-events-none disabled:opacity-50 aria-disabled:pointer-events-none aria-disabled:opacity-50 data-active:text-sidebar-accent-foreground [&_svg]:size-4 [&_svg]:shrink-0 [&>span:last-child]:truncate h-12 text-sm group-data-[collapsible=icon]:p-0! aria-expanded:bg-muted">
          <ThemeToggler />
        </div>
      </SidebarMenuItem>
    </SidebarMenu>
  );
}
