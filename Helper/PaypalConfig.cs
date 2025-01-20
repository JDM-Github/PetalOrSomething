using PayPalCheckoutSdk.Core;

namespace Helper {
    public static class PayPalConfig
    {
        public static PayPalEnvironment GetEnvironment()
        {
            var clientId = "AX5AGNee2pN271sDzuXWPldkFw9x_97bvDEKdpXcwNgmdE26uTD64JjEi2XRpY2AWKyUn29kicp1mocQ";
            var clientSecret = "EIKjEnAUbgNvX6ZswF0WurD8ocna3IAiF7TyHnL_T8B_548iOSGgomNiQxFNOX3fl8CuE9uXX_ojyNup";

            var environment = new LiveEnvironment(clientId, clientSecret);

            // var clientId = "Af0tB87keOdzXZpl_Ib8lb86Udu5oTWSL-xHDwAz4q9GiBQSFbejrkAqY2QQU5XAlYJ5PyFc6wsM45Wq";
            // var clientSecret = "EO6ufyuol6bxnX_E9HV9OmpqgD9SCWI5AEEohSaLYjBpJqbsVzv650YBQDWk7mZgPIPqE0IRpoQ5Gcyu";

            // var environment = new SandboxEnvironment(clientId, clientSecret);
            return environment;
        }
    }
}
