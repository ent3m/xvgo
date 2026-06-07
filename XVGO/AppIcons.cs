using Microsoft.FluentUI.AspNetCore.Components;

namespace XVGO;

internal static class AppIcons
{
    public class Sun : Icon
    {
        public Sun() : base(nameof(Sun), IconVariant.Regular, IconSize.Custom, """
            <svg xmlns="http://www.w3.org/2000/svg" fill="currentColor" viewBox="0 0 24 24"><path d="M17 12a5 5 0 1 1-10 0 5 5 0 0 1 10 0"/><path fill-rule="evenodd" d="M12 1.25a.75.75 0 0 1 .75.75v2a.75.75 0 0 1-1.5 0V2a.75.75 0 0 1 .75-.75M1.25 12a.75.75 0 0 1 .75-.75h2a.75.75 0 0 1 0 1.5H2a.75.75 0 0 1-.75-.75m18 0a.75.75 0 0 1 .75-.75h2a.75.75 0 0 1 0 1.5h-2a.75.75 0 0 1-.75-.75M12 19.25a.75.75 0 0 1 .75.75v2a.75.75 0 0 1-1.5 0v-2a.75.75 0 0 1 .75-.75" clip-rule="evenodd"/><g opacity=".7"><path d="M3.67 3.716a.75.75 0 0 1 1.059-.048L6.95 5.7a.75.75 0 0 1-1.012 1.107L3.717 4.775a.75.75 0 0 1-.048-1.06M20.332 3.716a.75.75 0 0 1-.047 1.06l-2.223 2.03A.75.75 0 1 1 17.05 5.7l2.222-2.032a.75.75 0 0 1 1.06.048M17.026 17.025a.75.75 0 0 1 1.06 0l2.223 2.222a.75.75 0 1 1-1.061 1.06l-2.222-2.222a.75.75 0 0 1 0-1.06M6.975 17.025a.75.75 0 0 1 0 1.06l-2.222 2.223a.75.75 0 0 1-1.06-1.06l2.222-2.223a.75.75 0 0 1 1.06 0"/></g></svg>
            """) { }
    }

    public class Moon : Icon
    {
        public Moon() : base(nameof(Moon), IconVariant.Regular, IconSize.Custom, """
            <svg xmlns="http://www.w3.org/2000/svg" fill="currentColor" viewBox="0 0 24 24"><path fill-rule="evenodd" d="M22 12c0 5.523-4.477 10-10 10a10 10 0 0 1-3.321-.564A9 9 0 0 1 8 18c0-2.22.805-4.254 2.138-5.824A6.5 6.5 0 0 0 15.5 15a6.5 6.5 0 0 0 5.567-3.143c.24-.396.933-.32.933.143" clip-rule="evenodd" opacity=".7"/><path d="M2 12c0 4.359 2.789 8.066 6.679 9.435A9 9 0 0 1 8 18c0-2.221.805-4.254 2.138-5.824A6.47 6.47 0 0 1 9 8.5a6.5 6.5 0 0 1 3.143-5.567C12.54 2.693 12.463 2 12 2 6.477 2 2 6.477 2 12"/></svg>
            """) { }
    }

    public class GitHub : Icon
    {
        public GitHub() : base(nameof(GitHub), IconVariant.Regular, IconSize.Custom, """
            <svg xmlns="http://www.w3.org/2000/svg" fill="currentColor" data-name="Layer 1" viewBox="0 0 24 24"><path d="M12 2.247a10 10 0 0 0-3.162 19.487c.5.088.687-.212.687-.475 0-.237-.012-1.025-.012-1.862-2.513.462-3.163-.613-3.363-1.175a3.64 3.64 0 0 0-1.025-1.413c-.35-.187-.85-.65-.013-.662a2 2 0 0 1 1.538 1.025 2.137 2.137 0 0 0 2.912.825 2.1 2.1 0 0 1 .638-1.338c-2.225-.25-4.55-1.112-4.55-4.937a3.9 3.9 0 0 1 1.025-2.688 3.6 3.6 0 0 1 .1-2.65s.837-.262 2.75 1.025a9.43 9.43 0 0 1 5 0c1.912-1.3 2.75-1.025 2.75-1.025a3.6 3.6 0 0 1 .1 2.65 3.87 3.87 0 0 1 1.025 2.688c0 3.837-2.338 4.687-4.562 4.937a2.37 2.37 0 0 1 .674 1.85c0 1.338-.012 2.413-.012 2.75 0 .263.187.575.687.475A10.005 10.005 0 0 0 12 2.247"/></svg>
            """) { }
    }

    public class Trashcan : Icon
    {
        public Trashcan() : base(nameof(Trashcan), IconVariant.Regular, IconSize.Custom, """
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" viewBox="0 0 24 24"><path d="M10 11v6M14 11v6" opacity=".7"/><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6M3 6h18M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/></svg>
            """) { }
    }

    public class Copy : Icon
    {
        public Copy() : base(nameof(Copy), IconVariant.Regular, IconSize.Custom, """
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" viewBox="0 0 24 24"><rect width="8" height="4" x="8" y="2" rx="1" ry="1"/><path d="M8 4H6a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2v-2M16 4h2a2 2 0 0 1 2 2v4"/><path d="M21 14H11m4-4-4 4 4 4" opacity=".7"/></svg>
            """) { }
    }

    public class Paste : Icon
    {
        public Paste() : base(nameof(Paste), IconVariant.Regular, IconSize.Custom, """
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" viewBox="0 0 24 24"><path d="M16 4h2a2 2 0 0 1 2 2v1.344M8 4H6a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h12a2 2 0 0 0 1.793-1.113"/><path d="M11 14h10m-4-4 4 4-4 4" opacity=".7"/><rect width="8" height="4" x="8" y="2" rx="1"/></svg>
            """) { }
    }

    public class Convert : Icon
    {
        public Convert() : base(nameof(Convert), IconVariant.Regular, IconSize.Size28, """
            <svg xmlns="http://www.w3.org/2000/svg" fill="currentColor" viewBox="0 0 24 24"><path d="M15 22.75a.752.752 0 0 1-.64-1.14l1.05-1.75a.751.751 0 1 1 1.29.77l-.27.45c2.76-.65 4.83-3.13 4.83-6.09 0-.41.34-.75.75-.75s.75.34.75.75c-.01 4.28-3.49 7.76-7.76 7.76M2 9.75c-.41 0-.75-.34-.75-.75 0-4.27 3.48-7.75 7.75-7.75a.752.752 0 0 1 .64 1.14L8.59 4.14c-.21.35-.67.47-1.03.25a.746.746 0 0 1-.25-1.03l.27-.45A6.26 6.26 0 0 0 2.75 9c0 .41-.34.75-.75.75"/><path d="M14.8 12.63v2.94c0 2.45-.98 3.43-3.43 3.43H8.43C5.98 19 5 18.02 5 15.57v-2.94c0-2.45.98-3.43 3.43-3.43h2.94c2.45 0 3.43.98 3.43 3.43" opacity=".4"/><path d="M15.57 5h-2.94c-2.41 0-3.39.96-3.42 3.32h2.16c2.94 0 4.3 1.37 4.3 4.3v2.16c2.37-.03 3.32-1.01 3.32-3.42V8.43C19 5.98 18.02 5 15.57 5" opacity=".4"/></svg>
            """) { }
    }

    public class XVGO : Icon
    {
        public XVGO() : base(nameof(XVGO), IconVariant.Regular, IconSize.Custom, """
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" stroke="#63b3ed" stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" viewBox="0 0 24 24"><path d="M22 5.15v3.7c0 2.25-.9 3.15-3.15 3.15h-2.7C13.9 12 13 11.1 13 8.85v-3.7C13 2.9 13.9 2 16.15 2h2.7C21.1 2 22 2.9 22 5.15" opacity=".4"/><path d="M11 15.15v3.7C11 21.1 10.1 22 7.85 22h-2.7C2.9 22 2 21.1 2 18.85v-3.7C2 12.9 2.9 12 5.15 12h2.7c2.25 0 3.15.9 3.15 3.15" opacity=".4"/><path d="M22 15c0 3.87-3.13 7-7 7l1.05-1.75"/><path d="M2 9c0-3.87 3.13-7 7-7L7.95 3.75"/><path d="m15.5 4 4 6"/><path d="m15.5 10 4-6"/><path d="M5 19.25c0 .414.336.75.75.75H7a1 1 0 0 0 1-1v-1a1 1 0 0 0-1-1H6a1 1 0 0 1-1-1v-1a1 1 0 0 1 1-1h1.25a.75.75 0 0 1 .75.75"/></svg>
            """) { }
    }

    public class History : Icon
    {
        public History() : base(nameof(History), IconVariant.Regular, IconSize.Custom, """
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" viewBox="0 0 24 24"><path d="M3 12a9 9 0 1 0 9-9 9.75 9.75 0 0 0-6.74 2.74L3 8"/><path d="M3 3v5h5M12 7v5l4 2"/></svg>
            """) { }
    }

    public class Settings : Icon
    {
        public Settings() : base(nameof(Settings), IconVariant.Regular, IconSize.Custom, """
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" viewBox="0 0 24 24"><path d="M9.671 4.136a2.34 2.34 0 0 1 4.659 0 2.34 2.34 0 0 0 3.319 1.915 2.34 2.34 0 0 1 2.33 4.033 2.34 2.34 0 0 0 0 3.831 2.34 2.34 0 0 1-2.33 4.033 2.34 2.34 0 0 0-3.319 1.915 2.34 2.34 0 0 1-4.659 0 2.34 2.34 0 0 0-3.32-1.915 2.34 2.34 0 0 1-2.33-4.033 2.34 2.34 0 0 0 0-3.831A2.34 2.34 0 0 1 6.35 6.051a2.34 2.34 0 0 0 3.319-1.915"/><circle cx="12" cy="12" r="3"/></svg>
            """) { }
    }

    public class Dictionary : Icon
    {
        public Dictionary() : base(nameof(Dictionary), IconVariant.Regular, IconSize.Custom, """
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" viewBox="0 0 24 24"><path d="M4 19.5v-15A2.5 2.5 0 0 1 6.5 2H19a1 1 0 0 1 1 1v18a1 1 0 0 1-1 1H6.5a1 1 0 0 1 0-5H20M10 6v7"/><path d="M10 6h3a2 2 0 1 1 0 4h-3M12.5 10l2.5 3"/></svg>
            """) { }
    }

    public class Standalone : Icon
    {
        public Standalone() : base(nameof(Standalone), IconVariant.Regular, IconSize.Custom, """
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" class="lucide lucide-box-icon lucide-box" viewBox="0 0 24 24"><path d="M21 8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16Z"/><path d="m3.3 7 8.7 5 8.7-5M12 22V12"/></svg>
            """) { }
    }

    public class Upload : Icon
    {
        public Upload() : base(nameof(Upload), IconVariant.Regular, IconSize.Custom, """
            <svg xmlns="http://www.w3.org/2000/svg" fill="currentColor" fill-rule="evenodd" clip-rule="evenodd" viewBox="0 0 24 24"><path d="M3 14.25a.75.75 0 0 1 .75.75c0 1.435.002 2.436.103 3.192.099.734.28 1.122.556 1.399.277.277.665.457 1.4.556.754.101 1.756.103 3.191.103h6c1.435 0 2.436-.002 3.192-.103.734-.099 1.122-.28 1.399-.556.277-.277.457-.665.556-1.4.101-.755.103-1.756.103-3.191a.75.75 0 0 1 1.5 0v.055c0 1.367 0 2.47-.116 3.337-.122.9-.38 1.658-.982 2.26s-1.36.86-2.26.982c-.867.116-1.97.116-3.337.116h-6.11c-1.367 0-2.47 0-3.337-.116-.9-.122-1.658-.38-2.26-.982s-.86-1.36-.981-2.26c-.117-.867-.117-1.97-.117-3.337V15a.75.75 0 0 1 .75-.75" opacity=".7"/><path d="M12 2.25a.75.75 0 0 1 .553.244l4 4.375a.75.75 0 1 1-1.107 1.012l-2.696-2.95V16a.75.75 0 0 1-1.5 0V4.932l-2.696 2.95a.75.75 0 0 1-1.108-1.013l4-4.375A.75.75 0 0 1 12 2.25"/></svg>
            """) { }
    }

    public class Sparkle : Icon
    {
        public Sparkle() : base(nameof(Sparkle), IconVariant.Regular, IconSize.Custom, """
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" opacity=".8" viewBox="0 0 24 24"><path d="M11.017 2.814a1 1 0 0 1 1.966 0l1.051 5.558a2 2 0 0 0 1.594 1.594l5.558 1.051a1 1 0 0 1 0 1.966l-5.558 1.051a2 2 0 0 0-1.594 1.594l-1.051 5.558a1 1 0 0 1-1.966 0l-1.051-5.558a2 2 0 0 0-1.594-1.594l-5.558-1.051a1 1 0 0 1 0-1.966l5.558-1.051a2 2 0 0 0 1.594-1.594z"/></svg>
            """) { }
    }
}