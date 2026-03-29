import { Search, User, ShoppingBag } from "lucide-react";
import {
    NavigationMenu,
    NavigationMenuList,
    NavigationMenuItem,
    NavigationMenuLink,
} from "@/components/ui/navigation-menu";

const BRAND_NAME = "CAMOMILA";

const navLinks = [
    { label: "LOJA", href: "/shop", accent: true, underline: false },
    { label: "NOSSA HISTÓRIA", href: "/about", accent: false, underline: false },
    { label: "BLOG", href: "/blog", accent: false, underline: true },
    { label: "PROMOÇÕES", href: "/promotions", accent: true, underline: true },
    { label: "CONTATO", href: "/contact", accent: false, underline: false },
];

const ShopNavigationBar = () => {
    return (
        <header className="border-t-2 border-t-[#b8a060] bg-white">
            <div className="mx-auto flex h-16 max-w-screen-xl items-center px-6">

                {/* Left — nav links */}
                <NavigationMenu className="flex-1 justify-start">
                    <NavigationMenuList className="gap-5">
                        {navLinks.map((link) => (
                            <NavigationMenuItem key={link.label}>
                                <NavigationMenuLink
                                    href={link.href}
                                    className={[
                                        "text-[11px] font-semibold tracking-widest transition-opacity hover:opacity-70",
                                        link.accent ? "text-[#4a7c7c]" : "text-[#1a1a1a]",
                                        link.underline ? "underline underline-offset-2" : "",
                                    ].join(" ")}
                                >
                                    {link.label}
                                </NavigationMenuLink>
                            </NavigationMenuItem>
                        ))}
                    </NavigationMenuList>
                </NavigationMenu>

                {/* Center — brand logo */}
                <div className="flex flex-col items-center justify-center border border-[#1a1a1a] px-8 py-2">
                    <span className="font-serif text-xl tracking-[0.35em] text-[#1a1a1a]">
                        {BRAND_NAME}
                    </span>
                </div>

                {/* Right — actions */}
                <div className="flex flex-1 items-center justify-end gap-5">
                    <button
                        aria-label="Search"
                        className="text-[#1a1a1a] transition-opacity hover:opacity-70"
                    >
                        <Search size={18} strokeWidth={1.5} />
                    </button>

                    <button
                        aria-label="Login"
                        className="flex items-center gap-1 text-[11px] font-semibold tracking-widest text-[#1a1a1a] transition-opacity hover:opacity-70"
                    >
                        <User size={18} strokeWidth={1.5} />
                        Login
                    </button>

                    <button
                        aria-label="Cart"
                        className="flex items-center gap-1 text-[11px] font-semibold text-[#1a1a1a] transition-opacity hover:opacity-70"
                    >
                        <ShoppingBag size={18} strokeWidth={1.5} />
                        <span>0</span>
                    </button>
                </div>

            </div>
        </header>
    );
};

export default ShopNavigationBar;
