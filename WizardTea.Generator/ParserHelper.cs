namespace WizardTea.Generator;

public static class ParserHelper {
    public static void RewriteValueBasedOnType(ref string value, string type) {
        switch (type) {
            case "float": {
                value += "f";

                break;
            }
        }
    }
}