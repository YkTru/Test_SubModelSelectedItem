using Application = System.Windows.Application;
#pragma warning disable CS8622

namespace test_subModelSelectedItem.WPF;

public partial class App : Application
{
    public App()
    {
        Activated += StartElmish;
    }

    private void StartElmish(object sender, EventArgs e)
    {
        Activated -= StartElmish;
        Program.Program.main(MainWindow);
    }
}