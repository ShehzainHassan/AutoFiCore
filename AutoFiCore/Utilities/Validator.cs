using AutoFiCore.Dto;
using AutoFiCore.Models;

namespace AutoFiCore.Utilities
{
    public class Validator
    {
        public static string? ValidatePagination(int pageView, int offset)
        {
            if (pageView <= 0)
                return "'pageView' must be greater than 0.";
            if (offset < 0)
                return "'offset' cannot be negative";
            return null;
        }
        public static string? ValidateMileage(int? mileage)
        {

            if (mileage.HasValue && mileage < 0)
                return "'mileage' must be greater than 0.";
            return null;
        }
        public static string? ValidateMakeOrModel(string makeORmodel)
        {
            if (string.IsNullOrWhiteSpace(makeORmodel) || makeORmodel is null)
                return "'make' is required.";

            return null;
        }
        public static bool ValidatePrice(decimal? startPrice, decimal? endPrice)
        {
            if (startPrice.HasValue && endPrice.HasValue)
            {
                return startPrice.Value <= endPrice.Value;
            }

            return true;
        }
        public static string? ValidateStringField(string value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return $"{fieldName} is required and cannot be empty.";
            }
            return null;
        }
        public static List<string> ValidateContactInfo(ContactInfo contactInfo)
        {
            var errors = new List<string>();

            void AddError(string? error)
            {
                if (!string.IsNullOrWhiteSpace(error))
                    errors.Add(error);
            }

            AddError(ValidateStringField(contactInfo.FirstName, "FirstName"));
            AddError(ValidateStringField(contactInfo.LastName, "LastName"));
            AddError(ValidateStringField(contactInfo.SelectedOption, "SelectedOption"));
            AddError(ValidateStringField(contactInfo.VehicleName, "VehicleName"));
            AddError(ValidateStringField(contactInfo.PostCode, "PostCode"));
            AddError(ValidateStringField(contactInfo.Email, "Email"));
            AddError(ValidateStringField(contactInfo.PhoneNumber, "PhoneNumber"));
            AddError(ValidateStringField(contactInfo.PreferredContactMethod, "PreferredContactMethod"));

            return errors;
        }
        private static bool IsValidEmail(string email)
        {
            var emailRegex = new System.Text.RegularExpressions.Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return emailRegex.IsMatch(email);
        }
        private static bool IsValidName(string name)
        {
            var nameRegex = new System.Text.RegularExpressions.Regex(@"^[A-Za-z\s]+$");
            return nameRegex.IsMatch(name);
        }
        private static bool IsStrongPassword(string password)
        {
            var passwordRegex = new System.Text.RegularExpressions.Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$");
            return passwordRegex.IsMatch(password);
        }
        public static List<string> ValidateUserInfo(User userInfo)
        {
            var errors = new List<string>();

            void AddError(string? error)
            {
                if (!string.IsNullOrWhiteSpace(error))
                    errors.Add(error);
            }

            AddError(ValidateStringField(userInfo.Email, "Email"));
            if (!string.IsNullOrWhiteSpace(userInfo.Email) && !IsValidEmail(userInfo.Email))
                errors.Add("Email is not in a valid format.");

            AddError(ValidateStringField(userInfo.Name, "Name"));
            if (!string.IsNullOrWhiteSpace(userInfo.Name) && !IsValidName(userInfo.Name))
                errors.Add("Name must only contain letters and spaces.");

            AddError(ValidateStringField(userInfo.Password, "Password"));
            if (!string.IsNullOrWhiteSpace(userInfo.Password) && !IsStrongPassword(userInfo.Password))
                errors.Add("Password must be at least 8 characters long and include at least one uppercase letter, one lowercase letter, one number, and one special character.");

            return errors;
        }
        public static string ValidateNewsLetter(string email)
        {
            if (!IsValidEmail(email))
            {
                return "Email is not in a valid format.";
            }
            if (string.IsNullOrWhiteSpace(email))
            {
                return "'Email' is required.";
            }
            return "";
        }
        public static List<string> ValidateFilters(VehicleFilterDto filters)
        {
            var errors = new List<string>();
            var mileageError = ValidateMileage(filters.Mileage);
            if (!string.IsNullOrWhiteSpace(mileageError))
                errors.Add(mileageError);

            if (filters.StartPrice.HasValue && filters.EndPrice.HasValue)
            {
                if (!ValidatePrice(filters.StartPrice, filters.EndPrice))
                {
                    errors.Add("'StartPrice' must be less than or equal to 'EndPrice'.");
                }
            }

            if (filters.StartYear.HasValue && filters.EndYear.HasValue)
            {
                if (filters.StartYear > filters.EndYear)
                {
                    errors.Add("'StartYear' must be less than or equal to 'EndYear'.");
                }
            }
            return errors;
        }

        public static List<string> ValidateAuctionDto(CreateAuctionDTO dto)
        {
            var errors = new List<string>();

            if (dto.VehicleId <= 0)
                errors.Add("VehicleId must be greater than 0.");

            if (dto.StartUtc == default)
                errors.Add("StartUtc cannot be empty.");

            if (dto.EndUtc == default)
                errors.Add("EndUtc cannot be empty.");

            if (dto.EndUtc <= dto.StartUtc)
                errors.Add("EndUtc must be <= StartUtc.");

            if (dto.StartingPrice < 0)
                errors.Add("Starting price must be non-negative.");

            return errors;
        }
    }
}
