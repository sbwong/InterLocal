//Form Validation Logic

// Error message interface for forms
export interface ErrorPackage {
    firstName: string;
    lastName: string;
    username: string;
    phone: string;
    email: string;
    password: string;
    confirmPassword: string;
}

// TODO: Duplicate username validation! Will need API call

/* Function takes in:
 *  fieldName: field to be tested
 *  value: value of the inputted field
 */
export default function formValidation(
    fieldName: string,
    value: string
): string {
    switch (fieldName) {
        case "confirmPassword": {
            if (value === "") return "Confirm password required.";
            else return "";
        }
        case "currentPassword": {
            if (value === "") return "Current password required.";
            else return "";
        }
        case "email": {
            if (value === "") return "Email required.";
            else if (!/^.+@.+\..+$/.test(value))
                return "Sorry, must be in form john@example.com. No special characters allowed.";
            else return "";
        }

        case "firstName": {
            if (value === "") return "First name required.";
            else if (
                !/^[a-zA-ZàáâäãåąčćęèéêëėįìíîïłńòóôöõøùúûüųūÿýżźñçčšžÀÁÂÄÃÅĄĆČĖĘÈÉÊËÌÍÎÏĮŁŃÒÓÔÖÕØÙÚÛÜŲŪŸÝŻŹÑßÇŒÆČŠŽ∂ð ,.'-]+$/.test(
                    value
                )
            )
                return "Sorry, invalid name entered.";
            else return "";
        }
        case "lastName": {
            if (value === "") return "Last name required.";
            else if (
                !/^[a-zA-ZàáâäãåąčćęèéêëėįìíîïłńòóôöõøùúûüųūÿýżźñçčšžÀÁÂÄÃÅĄĆČĖĘÈÉÊËÌÍÎÏĮŁŃÒÓÔÖÕØÙÚÛÜŲŪŸÝŻŹÑßÇŒÆČŠŽ∂ð ,.'-]+$/.test(
                    value
                )
            )
                return "Sorry, invalid name entered.";
            else return "";
        }
        case "password": {
            if (value === "") {
                return "Password required.";
            } else if (/\s/.test(value)) {
                return "Password cannot contain white space characters.";
            } else if (
                !(
                    value.length >= 8 &&
                    /[A-Za-z]/.test(value) &&
                    /[^A-Za-z]/.test(value)
                )
            ) {
                return "Password must have at least 8 characters and contain at least one letter and one non-letter character.";
            } else return "";
        }
        case "phone": {
            if (value === "") return "";
            else if (!/^[0-9]{10}$/.test(value))
                return "Sorry, only numbers (0-9) are allowed in form 1234567890.";
            else return "";
        }
        case "username": {
            if (value === "") return "Username required.";
            else if (!/^[A-Za-z0-9]+$/.test(value))
                return "Sorry, only letters (a-z), and numbers (0-9) are allowed.";
            else return "";
        }
        default:
            return "";
    }
}
