namespace PalServerTools.Components
{
    public class MenuItems
    {
        public string Title { get; set; }
        public string Path { get; set; }
        public string Icon { get; set; }
        public List<MenuItems>? Children { get; set; }
    }
}
