using MathChatBot.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NCalc;

namespace MathChatBot.Helpers
{

    /// <summary>
    /// Exceptions used in the SimpleCalculatorHelper
    /// </summary>
    #region Exceptions

    public class NegativeSqrtException : Exception
    {
        public NegativeSqrtException() : base() { }
        public NegativeSqrtException(string s) : base(s) { }
        public NegativeSqrtException(string s, Exception ex) : base(s, ex) { }
    }

    public class IllegalMathException : Exception
    {
        //Throws an exception for wrongful use of math in general.
        public IllegalMathException() : base() { }
        public IllegalMathException(string s) : base(s) { }
        public IllegalMathException(string s, Exception ex) : base(s, ex) { }
    }

    public class AcosWrongValueException : Exception
    {
        public AcosWrongValueException()
        {
        }

        public AcosWrongValueException(string message) : base(message)
        {
        }

        public AcosWrongValueException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class AsinWrongValueException : Exception
    {
        public AsinWrongValueException()
        {
        }

        public AsinWrongValueException(string message) : base(message)
        {
        }

        public AsinWrongValueException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    #endregion

    /// <summary>
    /// Interaction logic for the SimpleCalculatorHelper
    /// </summary>
    public class SimpleCalculatorHelper
    {

        //*************************************************/
        // VARIABLES
        //*************************************************/
        #region Variables

        private string _result;
        private string _input;

        #endregion

        //*************************************************/
        // PROPERTIES
        //*************************************************/
        #region Properties

        // The variable where all results are saved
        private string Result
        {
            get { return _result; }
            set
            {
                // This instance calculates the result for the string "value" which is entered by the user
                Expression e = new Expression(value, EvaluateOptions.IgnoreCase);
                var res = e.Evaluate();

                // Checking for wrongful math and throws exceptions accordingly
                if (res is double && double.IsInfinity((double)res))
                    throw new DivideByZeroException();
                if (res is double && double.IsNaN((double)res) && value.ToLower().Contains("sqrt(-"))
                    throw new NegativeSqrtException();
                else if (res is double && (double.IsNaN((double)res) || double.IsInfinity((double)res)))
                {
                    if (value.ToLower().Contains("acos("))
                        throw new AcosWrongValueException();

                    else if (value.ToLower().Contains("asin("))
                        throw new AsinWrongValueException();

                    else
                        throw new IllegalMathException();

                }
                // The final result is calculated and stored
                _result = e.Evaluate().ToString().Replace(",", ".");
            }
        }

        // Stores the input of the user
        private string Input
        {
            get { return _input; }
            set
            {
                _input = value;
                // Replaces the input from the user with valid input to make sure our library can calculate the result
                _input = ReplaceNaturalLanguage(_input);
                
                // If the inputstring starts with '=' it will overwrite the current result
                if (_input.StartsWith("="))
                    Result = _input.Substring(1);
                else if (!_input.StartsWith("+") && !_input.StartsWith("-") && !_input.StartsWith("*") && !_input.StartsWith("/") && _input != "value")
                    Result = Result + "+" + _input;
                else
                    Result += string.Format("{0}", _input);
            }
        }

        #endregion

        //*************************************************/
        // METHODS
        //*************************************************/
        #region Methods

        /// <summary>
        /// Replace natural in the given input with math operators
        /// </summary>
        /// <param name="input">The given input text</param>
        /// <param name="removeSpaces">Flag for removing spaces in the input text</param>
        /// <returns></returns>
        public string ReplaceNaturalLanguage(string input, bool removeSpaces = true)
        {
            input = input.ReplaceIgnoreCase("add", "+");
            input = input.ReplaceIgnoreCase("plus", "+");
            input = input.ReplaceIgnoreCase("minus", "-");
            input = input.ReplaceIgnoreCase("subtract", "-");
            input = input.ReplaceIgnoreCase("times", "*");
            input = input.ReplaceIgnoreCase("multiply by", "*");
            input = input.ReplaceIgnoreCase("multiply", "*");
            input = input.ReplaceIgnoreCase("divide by", "/");
            input = input.ReplaceIgnoreCase("divide", "/");
            input = input.ReplaceIgnoreCase("modulus", "%");
            if (removeSpaces)
                input = input.ReplaceIgnoreCase(" ", "");
            return input;
        }

        /// <summary>
        /// Uses all the methods for the calculator.
        /// </summary>
        /// <param name="calculation">The user input</param>
        /// <returns></returns>
        public string UseCalculator(string calculation)
        {
            if (Result == null || Result == "")
                Result = "0";

            calculation = calculation.ReplaceIgnoreCase("value", Result);
            if (IsCalculatorCommand(calculation))
                return Properties.Resources.your_total_value_has_been_set_to_0;
            else if (!HasNumber(calculation))
                return null;

            try
            {
                Input = calculation;
            }
            catch (ArgumentException)
            {
                return Properties.Resources.please_enter_proper_math_function;
            }
            catch (DivideByZeroException)
            {
                return Properties.Resources.you_cannot_divide_by_zero;
            }
            catch (NegativeSqrtException)
            {
                return Properties.Resources.you_cannot_take_square_root_of_negative_number;
            }
            catch (AcosWrongValueException)
            {
                return Properties.Resources.acos_can_only_contain_numbers_between_minus1_and_1;
            }
            catch (AsinWrongValueException)
            {
                return Properties.Resources.asin_can_only_contain_numbers_between_minus1_and_1;
            }
            catch (IllegalMathException)
            {
                return Properties.Resources.please_perform_a_correct_and_legal_math_operation;
            }
            catch (EvaluationException)
            {
                return Properties.Resources.please_enter_valid_input_in_functions;
            }
            catch (Exception)
            {
                return null;
            }

            return Result;
        }

        /// <summary>
        /// Checks for calculatorcommands
        /// </summary>
        /// <param name="input">The inputstring from the user</param>
        /// <returns>Returns true or false depending if the string is a calculator command or not.</returns>
        private bool IsCalculatorCommand(string input)
        {
            switch (input)
            {
                case "clear result":
                    {
                        Result = "0";
                        return true;
                    }
                default:
                    return false;
            }
        }

        /// <summary>
        /// Checks is the inputstring contains a number
        /// </summary>
        /// <param name="input">The inputstring from the user</param>
        /// <returns>Returns true or false depending if the inputstring contains a number or not</returns>
        public bool HasNumber(string input)
        {
            return input.Any(c => char.IsDigit(c));
        }

        #endregion

    }

}
