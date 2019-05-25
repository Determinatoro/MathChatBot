using MathChatBot.Utilities;
using NCalc;
using System;
using System.Collections.Generic;
using System.Linq;

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
                e.EvaluateFunction += (sender, args) =>
                {
                    try
                    {
                        var functionName = sender.ToLower();

                        switch (functionName)
                        {
                            case "cos":
                            case "tan":
                            case "sin":
                                {
                                    // Get method for the function
                                    var methodInfo = typeof(Math).GetMethods().FirstOrDefault(x => x.Name.ToLower() == functionName);
                                    // Get input
                                    var input = args.Parameters[0].Evaluate();

                                    // Convert to radians
                                    var v = Convert.ToDouble(input);
                                    var radians = (v * Math.PI) / 180;

                                    // Calculate
                                    var result = methodInfo.Invoke(null, new object[] { radians });

                                    // Value
                                    var val = Convert.ToDouble(result);
                                    val = Math.Round(val, 5);

                                    // Set result
                                    args.Result = val;

                                    break;
                                }
                            case "acos":
                            case "asin":
                            case "atan":
                                {
                                    // Get method for the function
                                    var methodInfo = typeof(Math).GetMethods().FirstOrDefault(x => x.Name.ToLower() == functionName);
                                    // Get input
                                    var input = args.Parameters[0].Evaluate();

                                    // Convert to double
                                    var v = Convert.ToDouble(input);

                                    // Calculate
                                    var result = methodInfo.Invoke(null, new object[] { v });

                                    // Convert to degrees
                                    var radians = Convert.ToDouble(result);
                                    var degrees = Math.Round((radians * 180) / Math.PI, 5);
                                    args.Result = degrees;

                                    break;
                                }
                        }
                    }
                    catch
                    {
                        args.Result = double.NaN;
                    }
                };
                var res = e.Evaluate();

                // Checking for wrongful math and throws exceptions accordingly
                if (res is double && double.IsInfinity((double)res))
                    throw new DivideByZeroException();
                else if (res is double && double.IsNaN((double)res))
                {
                    if (value.ToLower().Contains("acos("))
                        throw new AcosWrongValueException();
                    else if (value.ToLower().Contains("asin("))
                        throw new AsinWrongValueException();
                    else if (value.ToLower().Contains("sqrt(-"))
                        throw new NegativeSqrtException();
                    // Last resort if something is wrong
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

                // If the inputstring starts with '=' it will overwrite the current result
                if (_input.StartsWith("="))
                    Result = _input.Substring(1);
                else if (!_input.StartsWith("+") && !_input.StartsWith("-") && !_input.StartsWith("*") && !_input.StartsWith("/") && _input != Properties.Resources.value)
                    Result = Result + "+" + _input;
                else
                    Result += string.Format("{0}", _input);
            }
        }
        private List<ChatBotCommand> Commands { get; set; }

        #endregion

        //*************************************************/
        // CONSTRUCTOR
        //*************************************************/
        #region Contructor

        public SimpleCalculatorHelper()
        {
            GetCommands();
        }

        #endregion

        //*************************************************/
        // METHODS
        //*************************************************/
        #region Methods

        private void GetCommands()
        {
            Commands = new List<ChatBotCommand>();
            Commands.Add(new ChatBotCommand(new string[] {
                    Properties.Resources.clear_result.ToLower()
                },
                () =>
                {
                    Result = "0";
                    return Properties.Resources.your_total_value_has_been_set_to_0;
                }));
        }

        /// <summary>
        /// Check input
        /// </summary>
        /// <param name="input">Input given from the user</param>
        public bool CheckInput(ref string input, out string text)
        {
            if (RunCommand(input, out text))
                return true;

            var hasNumber = HasNumber(input);

            if (hasNumber)
                return true;
            else if (!hasNumber && input.Contains(Properties.Resources.value.ToLower()))
            {
                // Replace natural language in the text with operators
                input = ReplaceNaturalLanguage(input, removeSpaces: false);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Replace natural in the given input with math operators
        /// </summary>
        /// <param name="input">The given input text</param>
        /// <param name="removeSpaces">Flag for removing spaces in the input text</param>
        /// <returns></returns>
        public string ReplaceNaturalLanguage(string input, bool removeSpaces = true)
        {
            if (Result == null || Result == "")
                Result = "0";
            input = input.ReplaceIgnoreCase(Properties.Resources.value, Result);
            input = input.ReplaceIgnoreCase(Properties.Resources.add, "+");
            input = input.ReplaceIgnoreCase(Properties.Resources.plus, "+");
            input = input.ReplaceIgnoreCase(Properties.Resources.minus, "-");
            input = input.ReplaceIgnoreCase(Properties.Resources.substract, "-");
            input = input.ReplaceIgnoreCase(Properties.Resources.times, "*");
            input = input.ReplaceIgnoreCase(Properties.Resources.multiply_by, "*");
            input = input.ReplaceIgnoreCase(Properties.Resources.multiply, "*");
            input = input.ReplaceIgnoreCase(Properties.Resources.divide_by, "/");
            input = input.ReplaceIgnoreCase(Properties.Resources.divide, "/");
            input = input.ReplaceIgnoreCase(Properties.Resources.modulus, "%");
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
            var text = "";
            if (RunCommand(calculation, out text))
                return text;

            calculation = ReplaceNaturalLanguage(calculation);
            if (!HasNumber(calculation))
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
        private bool RunCommand(string input, out string text)
        {
            var command = Commands.FirstOrDefault(x => x.CommandTexts.Contains(input));
            text = command?.RunFunc();
            return command != null;
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
