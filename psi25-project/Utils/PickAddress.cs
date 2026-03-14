namespace psi25_project.Utils
{
    public static class AddressProvider
    {
        public static string GetRandomAddress(string filePath = "addresses.txt")
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Address file not found: {filePath}");

            string? selectedAddress = null;
            int count = 0;

            using (var reader = new StreamReader(filePath))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    count++;
                    if (Random.Shared.Next(count) == 0) 
                        selectedAddress = line;
                }
            }
            if (selectedAddress == null)
                throw new Exception("No valid address found.");
            return selectedAddress!;
        }
    }
}