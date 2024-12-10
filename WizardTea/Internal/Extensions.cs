namespace WizardTea.Internal;

internal static class Extensions {
    // c.f: https://stackoverflow.com/a/521894
    internal static void Each<T>(this IEnumerable<T> collection, Action<T, int> action) {
        var i = 0;
        foreach (var elem in collection) {
            action(elem, i++);
        }
    }
}