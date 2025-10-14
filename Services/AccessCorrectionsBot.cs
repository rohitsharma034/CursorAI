using InmateSearchWebApp.Models;
using Microsoft.Playwright;


namespace TarrantInmateBotPlaywright;

public class AccessCorrectionsBot
{
    private readonly AccessCorrectionsOptions _opts;

    public AccessCorrectionsBot(AccessCorrectionsOptions opts)
    {
        if (opts == null) throw new ArgumentNullException(nameof(opts));
        if (string.IsNullOrWhiteSpace(opts.Username) || string.IsNullOrWhiteSpace(opts.Password))
            throw new ArgumentException("Username and Password are required in UserOptions.");

        _opts = opts;
    }

    public async Task<bool> LoginOrSignupAsync(IPage page)
    {
        try
        {
            await page.GotoAsync("https://www.accesscorrections.com/");

            // ✅ Accept cookies if banner exists
            var cookieBtn = page.GetByRole(AriaRole.Button, new() { Name = "OK" });
            if (await cookieBtn.CountAsync() == 0)
                cookieBtn = page.GetByText("OK");

            if (await cookieBtn.CountAsync() > 0)
            {
                await cookieBtn.First.ClickAsync();
                await page.WaitForTimeoutAsync(500);
            }

            // ✅ Click SIGN IN on homepage
            var signInBtn = page.GetByRole(AriaRole.Button, new() { Name = "Sign In" });
            if (await signInBtn.CountAsync() == 0)
                signInBtn = page.GetByRole(AriaRole.Link, new() { Name = "Sign In" });

            if (await signInBtn.CountAsync() > 0)
            {
                await signInBtn.First.ClickAsync();
            }
            else
            {
                Console.WriteLine("⚠️ SIGN IN button not found.");
                return false;
            }

            // ✅ Wait for login form
            var emailInput = page.GetByLabel("E-mail");
            if (await emailInput.CountAsync() == 0)
                emailInput = page.Locator("input[type=email]");

            var passwordInput = page.GetByLabel("Password");
            if (await passwordInput.CountAsync() == 0)
                passwordInput = page.Locator("input[type=password]");

            // ✅ Fill login form
            await emailInput.FillAsync(_opts.Username ?? "");
            await passwordInput.FillAsync(_opts.Password ?? "");

            // ✅ Submit login
            var submitBtn = page.GetByRole(AriaRole.Button, new() { Name = "Login" });
            if (await submitBtn.CountAsync() == 0)
                submitBtn = page.Locator("button[type=submit], input[type=submit]");

            await submitBtn.ClickAsync();
            await page.WaitForTimeoutAsync(4000);

            // ✅ Check for login error message
            var loginError = page.GetByText("Incorrect username or password");
            if (await loginError.CountAsync() > 0)
            {
                Console.WriteLine("⚠️ Login failed - incorrect credentials detected. Proceeding to signup...");
            }
            else
            {
                // ✅ Login success check
                if (await page.GetByText("Send Money").CountAsync() > 0 ||
                    await page.GetByRole(AriaRole.Link, new() { Name = "Log Out" }).CountAsync() > 0)
                {
                    Console.WriteLine("✅ Login successful.");
                    return true;
                }
            }

            // ✅ Login failed → try Sign Up
            Console.WriteLine("⚠️ Login failed. Checking for Sign Up option...");

            // Try multiple ways to find the Sign Up button/link
            var signUpBtn = page.GetByRole(AriaRole.Link, new() { Name = "Sign Up" });
            if (await signUpBtn.CountAsync() == 0)
                signUpBtn = page.GetByRole(AriaRole.Button, new() { Name = "Sign Up" });
            if (await signUpBtn.CountAsync() == 0)
                signUpBtn = page.GetByText("Sign Up");
            if (await signUpBtn.CountAsync() == 0)
                signUpBtn = page.Locator("a:has-text('Sign Up'), button:has-text('Sign Up')");

            if (await signUpBtn.CountAsync() > 0)
            {
                Console.WriteLine("✅ Found Sign Up button. Clicking...");
                await signUpBtn.First.ClickAsync();
                await page.WaitForTimeoutAsync(2000);

                // ✅ Fill signup form - Personal Information step
                Console.WriteLine("📝 Filling Personal Information form...");

                // Clear and fill First Name
                var firstNameField = page.GetByLabel("First Name *");
                await firstNameField.ClearAsync();
                await firstNameField.FillAsync(_opts.FirstName ?? "");

                // Handle middle name - check if user has middle name or check the "no middle name" checkbox
                if (string.IsNullOrEmpty(_opts.MiddleName))
                {
                    // Check the "Check if no middle name" checkbox
                    var noMiddleNameCheckbox = page.GetByText("Check if no middle name");
                    if (await noMiddleNameCheckbox.CountAsync() > 0)
                    {
                        var isChecked = await noMiddleNameCheckbox.IsCheckedAsync();
                        if (!isChecked)
                        {
                            await noMiddleNameCheckbox.ClickAsync();
                            Console.WriteLine("✅ Checked 'no middle name' checkbox");
                        }
                    }
                }
                else
                {
                    // Uncheck the "no middle name" checkbox first
                    var noMiddleNameCheckbox = page.GetByText("Check if no middle name");
                    if (await noMiddleNameCheckbox.CountAsync() > 0)
                    {
                        var isChecked = await noMiddleNameCheckbox.IsCheckedAsync();
                        if (isChecked)
                        {
                            await noMiddleNameCheckbox.ClickAsync();
                        }
                    }
                    // Fill middle name
                    await page.GetByLabel("Middle Name *").FillAsync(_opts.MiddleName);
                }

                // Clear and fill Last Name
                var lastNameField = page.GetByLabel("Last Name *");
                await lastNameField.ClearAsync();
                await lastNameField.FillAsync(_opts.LastName ?? "");

                // Fill Date of Birth
                await page.GetByLabel("Date of Birth *").FillAsync(_opts.DateOfBirth ?? "");

                // Clear and fill Phone
                var phoneField = page.GetByLabel("Phone *");
                await phoneField.ClearAsync();
                await phoneField.FillAsync(_opts.Phone ?? "");

                // Clear and fill Email
                var emailValue = _opts.Username ?? "";
                // Prefer a stable attribute to avoid strict-mode collisions
                var emailField = page.Locator("[data-id='awctstel_personalInfo_email_textfield']");
                if (await emailField.CountAsync() == 0)
                {
                    emailField = page.GetByLabel("Email *").First;
                }
                await emailField.ClearAsync();
                await emailField.FillAsync(emailValue);

                // Fill Confirm Email (with fallbacks) and verify value
                ILocator confirmEmailField;
                // Prefer a stable attribute first, then fall back to label
                confirmEmailField = page.Locator("[data-id='awctstel_personalInfo_confirmEmail_textfield']");
                if (await confirmEmailField.CountAsync() == 0)
                    confirmEmailField = page.GetByLabel("Confirm Email *").First;
                if (await confirmEmailField.CountAsync() == 0)
                    confirmEmailField = page.GetByLabel("Confirm Email");
                if (await confirmEmailField.CountAsync() == 0)
                    confirmEmailField = page.Locator("input[placeholder*='Confirm'][type='email'], input[name*='confirm'][type='email']");
                if (await confirmEmailField.CountAsync() > 0)
                {
                    await confirmEmailField.ClearAsync();
                    await confirmEmailField.ClickAsync();
                    // Some sites block paste in Confirm Email. Type it character-by-character instead.
                    await confirmEmailField.TypeAsync(emailValue, new LocatorTypeOptions { Delay = 30 });
                    var confirmVal = await confirmEmailField.InputValueAsync();
                    if (!string.Equals(confirmVal, emailValue, StringComparison.OrdinalIgnoreCase))
                    {
                        // Retry typing once more if mismatch
                        await confirmEmailField.ClearAsync();
                        await confirmEmailField.TypeAsync(emailValue, new LocatorTypeOptions { Delay = 30 });
                    }
                }

                // ✅ Accept terms and conditions (required) - this is critical!
                Console.WriteLine("📋 Accepting terms and conditions...");
                var termsCheckbox = page.Locator("input[type='checkbox']").Filter(new() { HasText = "I accept the user agreement and terms and conditions" });
                if (await termsCheckbox.CountAsync() == 0)
                {
                    // Try alternative selector - find checkbox near the terms text
                    var termsText = page.GetByText("I accept the user agreement and terms and conditions");
                    if (await termsText.CountAsync() > 0)
                    {
                        // Look for checkbox in the same parent container
                        termsCheckbox = termsText.Locator("..").Locator("input[type='checkbox']");
                    }
                }
                if (await termsCheckbox.CountAsync() == 0)
                {
                    // Try finding by looking for checkbox with specific attributes
                    termsCheckbox = page.Locator("input[type='checkbox'][name*='terms'], input[type='checkbox'][name*='agreement'], input[type='checkbox'][id*='terms']");
                }

                bool termsSet = false;
                if (await termsCheckbox.CountAsync() > 0)
                {
                    try
                    {
                        await termsCheckbox.ScrollIntoViewIfNeededAsync();
                        await termsCheckbox.CheckAsync(new() { Force = true });
                        termsSet = await termsCheckbox.IsCheckedAsync();
                    }
                    catch
                    {
                        // fall through
                    }
                }
                if (!termsSet)
                {
                    Console.WriteLine("⚠️ Could not directly check checkbox; trying role-based locator");
                    var roleCheckbox = page.GetByRole(AriaRole.Checkbox, new() { Name = "I accept the user agreement and terms and conditions" });
                    if (await roleCheckbox.CountAsync() == 0)
                        roleCheckbox = page.GetByRole(AriaRole.Checkbox, new() { NameRegex = new System.Text.RegularExpressions.Regex("I accept.*terms", System.Text.RegularExpressions.RegexOptions.IgnoreCase) });
                    if (await roleCheckbox.CountAsync() > 0)
                    {
                        try
                        {
                            await roleCheckbox.ScrollIntoViewIfNeededAsync();
                            await roleCheckbox.CheckAsync(new() { Force = true });
                            termsSet = await roleCheckbox.IsCheckedAsync();
                        }
                        catch { }
                    }
                }
                if (!termsSet)
                {
                    Console.WriteLine("⚠️ Role-based check failed; trying to click the label text");
                    var termsLabel = page.GetByText("I accept the user agreement and terms and conditions");
                    if (await termsLabel.CountAsync() > 0)
                    {
                        try
                        {
                            await termsLabel.ScrollIntoViewIfNeededAsync();
                            await termsLabel.ClickAsync();
                            // probe any nearby checkbox state
                            var anyCheckbox = page.Locator("input[type='checkbox']");
                            if (await anyCheckbox.CountAsync() > 0)
                            {
                                termsSet = await anyCheckbox.First.IsCheckedAsync();
                            }
                        }
                        catch { }
                    }
                }
                if (!termsSet)
                {
                    Console.WriteLine("⚠️ Fallback to JS to set checkbox and dispatch events");
                    await page.EvaluateAsync(@"() => {
                  const label = Array.from(document.querySelectorAll('label, div, span, p'))
                    .find(el => /I accept the user agreement.*terms and conditions/i.test(el.textContent||''));
                  let checkbox = label ? label.closest('div')?.querySelector('input[type=checkbox]') : null;
                  if (!checkbox) checkbox = document.querySelector('input[type=checkbox]');
                  if (checkbox) {
                    checkbox.checked = true;
                    checkbox.dispatchEvent(new Event('input', { bubbles: true }));
                    checkbox.dispatchEvent(new Event('change', { bubbles: true }));
                  }
                }");
                    var anyCheckbox = page.Locator("input[type='checkbox']");
                    if (await anyCheckbox.CountAsync() > 0)
                    {
                        termsSet = await anyCheckbox.First.IsCheckedAsync();
                    }
                }
                Console.WriteLine(termsSet ? "✅ Terms accepted" : "❌ Unable to set terms checkbox");

                // ✅ Wait a moment for any validation to complete
                await page.WaitForTimeoutAsync(1000);

                // Check for any validation errors before proceeding
                var validationError = page.GetByText("*Please indicate that you have read and agree to the terms and conditions");
                if (await validationError.CountAsync() > 0)
                {
                    Console.WriteLine("❌ Terms and conditions not accepted. Trying to fix...");

                    // Try clicking the checkbox again
                    var termsCheckboxRetry = page.Locator("input[type='checkbox']").Filter(new() { HasText = "I accept the user agreement and terms and conditions" });
                    if (await termsCheckboxRetry.CountAsync() == 0)
                    {
                        var termsText = page.GetByText("I accept the user agreement and terms and conditions");
                        if (await termsText.CountAsync() > 0)
                        {
                            termsCheckboxRetry = termsText.Locator("..").Locator("input[type='checkbox']");
                        }
                    }
                    if (await termsCheckboxRetry.CountAsync() > 0)
                    {
                        await termsCheckboxRetry.ClickAsync();
                        await page.WaitForTimeoutAsync(500);
                    }
                }

                // Ensure confirm email and terms are applied before clicking NEXT
                await page.WaitForTimeoutAsync(300);
                Console.WriteLine("🚀 Clicking NEXT button...");
                var nextBtn = page.GetByRole(AriaRole.Button, new() { Name = "NEXT" });
                if (await nextBtn.CountAsync() == 0)
                    nextBtn = page.GetByText("NEXT");
                if (await nextBtn.CountAsync() == 0)
                    nextBtn = page.Locator("button:has-text('NEXT')");

                if (await nextBtn.CountAsync() > 0)
                {
                    // Check if button is enabled
                    var isEnabled = await nextBtn.IsEnabledAsync();
                    if (!isEnabled)
                    {
                        Console.WriteLine("⚠️ NEXT button is disabled. Checking for validation errors...");

                        // Check for specific validation errors
                        var termsError = page.GetByText("*Please indicate that you have read and agree to the terms and conditions");
                        if (await termsError.CountAsync() > 0)
                        {
                            Console.WriteLine("❌ Terms and conditions error still present");
                            return false;
                        }

                        // Check for other validation errors
                        var otherErrors = page.Locator(".error, .validation-error, [class*='error']");
                        var errorCount = await otherErrors.CountAsync();
                        if (errorCount > 0)
                        {
                            Console.WriteLine($"❌ Found {errorCount} validation errors on the form");
                            return false;
                        }
                        // As a last attempt, try re-setting confirm email and clicking the terms label
                        if (await confirmEmailField.CountAsync() > 0)
                        {
                            await confirmEmailField.ClearAsync();
                            await confirmEmailField.TypeAsync(emailValue, new LocatorTypeOptions { Delay = 30 });
                        }
                        var termsLabelRetry = page.GetByText("I accept the user agreement and terms and conditions");
                        if (await termsLabelRetry.CountAsync() > 0)
                        {
                            await termsLabelRetry.ClickAsync();
                        }
                        await page.WaitForTimeoutAsync(300);
                        isEnabled = await nextBtn.IsEnabledAsync();
                        if (!isEnabled)
                        {
                            Console.WriteLine("❌ NEXT still disabled after retries");
                            return false;
                        }
                    }

                    await nextBtn.ClickAsync();
                    await page.WaitForTimeoutAsync(3000);

                    // Check if we're on the next step or if there are validation errors
                    if (await page.GetByText("Please be sure to enter the correct information").CountAsync() > 0)
                    {
                        Console.WriteLine("❌ Signup form validation failed. Please check the entered information.");
                        return false;
                    }

                    // Check if we moved to the next step
                    var currentUrl = page.Url;
                    var pageTitle = await page.TitleAsync();
                    Console.WriteLine($"Current URL: {currentUrl}");
                    Console.WriteLine($"Page title: {pageTitle}");

                    // If we reach here, the first step was successful
                    Console.WriteLine("✅ Personal Information step completed. Proceeding to Billing Address...");

                    // =====================
                    // Step 2: Billing Address
                    // =====================
                    try
                    {
                        // Wait for address fields to appear
                        await page.WaitForSelectorAsync("text=Billing Address", new() { Timeout = 5000 });

                        // Address line
                        var addrField = page.GetByLabel("Address *");
                        if (await addrField.CountAsync() == 0)
                            addrField = page.Locator("input[placeholder*='Address']").First;
                        await addrField.FillAsync(_opts.Address ?? "");
                        // Handle address autocomplete list (select first suggestion)
                        try
                        {
                            var listbox = page.Locator("[role='listbox'], ul[role='presentation']");
                            await listbox.First.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 1500 });
                            var firstOption = page.GetByRole(AriaRole.Option).First;
                            if (await firstOption.CountAsync() > 0)
                            {
                                await firstOption.ClickAsync();
                            }
                            else
                            {
                                // Alternative: press ArrowDown + Enter
                                await addrField.PressAsync("ArrowDown");
                                await addrField.PressAsync("Enter");
                            }
                        }
                        catch { /* autocomplete may not appear; ignore */ }

                        // City
                        var cityField = page.GetByLabel("City *");
                        if (await cityField.CountAsync() == 0)
                            cityField = page.Locator("input[placeholder*='City']").First;
                        await cityField.FillAsync(_opts.City ?? "");
                        // Dismiss any city autocomplete to unblock focus
                        try
                        {
                            await cityField.PressAsync("Enter");
                        }
                        catch { }

                        // State (dropdown or select) - robust resolution
                        ILocator stateCombobox = page.GetByLabel("State *").First;
                        if (await stateCombobox.CountAsync() == 0)
                        {
                            // Try associating via label 'for' attribute
                            var stateLabel = page.Locator("label:has-text('State')").First;
                            if (await stateLabel.CountAsync() > 0)
                            {
                                var forAttr = await stateLabel.GetAttributeAsync("for");
                                if (!string.IsNullOrEmpty(forAttr))
                                {
                                    stateCombobox = page.Locator("#" + forAttr);
                                }
                                if (await stateCombobox.CountAsync() == 0)
                                {
                                    // Search within same container
                                    stateCombobox = stateLabel.Locator("..").Locator("input, div[role='combobox'], [role='button'][aria-haspopup='listbox'], select").First;
                                }
                            }
                        }
                        if (await stateCombobox.CountAsync() > 0)
                        {
                            await stateCombobox.ScrollIntoViewIfNeededAsync();
                            var tag = await stateCombobox.EvaluateAsync<string>("el => el.tagName.toLowerCase()");
                            var stateValue = _opts.State ?? "";
                            if (tag == "select")
                            {
                                try
                                {
                                    await stateCombobox.SelectOptionAsync(new[] { stateValue });
                                }
                                catch
                                {
                                    await stateCombobox.SelectOptionAsync(new SelectOptionValue() { Label = stateValue });
                                }
                            }
                            else
                            {
                                await stateCombobox.ClickAsync();
                                // Try clicking matching option
                                var stateOption = page.GetByRole(AriaRole.Option, new() { Name = stateValue });
                                if (await stateOption.CountAsync() == 0)
                                    stateOption = page.GetByText(stateValue, new() { Exact = true });
                                if (await stateOption.CountAsync() > 0)
                                {
                                    await stateOption.ClickAsync();
                                }
                                else
                                {
                                    // Keyboard fallback
                                    await stateCombobox.TypeAsync(stateValue);
                                    await stateCombobox.PressAsync("Enter");
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("⚠️ Could not locate State control; skipping selection");
                        }

                        // Zip Code
                        var zipField = page.GetByLabel("Zip Code *");
                        if (await zipField.CountAsync() == 0)
                            zipField = page.Locator("input[placeholder*='Zip']").First;
                        await zipField.FillAsync(_opts.Zip ?? "");
                        // Sometimes typing ZIP triggers suggestion; press Enter to commit
                        try { await zipField.PressAsync("Enter"); } catch { }

                        // Click NEXT
                        var nextBtn2 = page.GetByRole(AriaRole.Button, new() { Name = "NEXT" });
                        if (await nextBtn2.CountAsync() == 0)
                            nextBtn2 = page.GetByText("NEXT");
                        if (await nextBtn2.CountAsync() == 0)
                            nextBtn2 = page.Locator("button:has-text('NEXT')");
                        var enabled2 = await nextBtn2.IsEnabledAsync();
                        if (!enabled2)
                        {
                            Console.WriteLine("⚠️ NEXT disabled on Billing Address; checking validations...");
                            var addrErrors = page.Locator(".error, .validation-error, [class*='error']");
                            var cnt = await addrErrors.CountAsync();
                            if (cnt > 0) Console.WriteLine($"❌ Found {cnt} address validation errors");
                        }
                        await nextBtn2.ClickAsync();
                        await page.WaitForTimeoutAsync(2000);

                        // Handle USPS Suggested Address modal if it appears
                        try
                        {
                            var suggestDialog = page.Locator("text=Suggested Address").First;
                            if (await suggestDialog.CountAsync() == 0)
                            {
                                // try role-based
                                suggestDialog = page.GetByRole(AriaRole.Dialog, new() { Name = "Suggested Address" });
                            }
                            if (await suggestDialog.CountAsync() > 0)
                            {
                                Console.WriteLine("ℹ️ Suggested Address dialog detected. Selecting suggested address...");
                                // Prefer selecting the radio near the 'Suggested Address' column; otherwise choose last radio
                                var suggestedColumn = page.Locator("text=Suggested Address").First;
                                ILocator radio = null;
                                if (await suggestedColumn.CountAsync() > 0)
                                {
                                    radio = suggestedColumn.Locator("..").Locator("input[type='radio']").First;
                                }
                                if (radio == null || await radio.CountAsync() == 0)
                                {
                                    radio = page.Locator("input[type='radio']").Last;
                                }
                                if (await radio.CountAsync() > 0)
                                {
                                    try { await radio.CheckAsync(new() { Force = true }); } catch { await radio.ClickAsync(); }
                                }
                                // Click CONTINUE
                                var continueBtn = page.GetByRole(AriaRole.Button, new() { Name = "CONTINUE" });
                                if (await continueBtn.CountAsync() == 0)
                                    continueBtn = page.GetByText("CONTINUE");
                                if (await continueBtn.CountAsync() > 0)
                                {
                                    await continueBtn.ClickAsync();
                                    await page.WaitForTimeoutAsync(1500);
                                }
                            }
                        }
                        catch { }
                        // =====================
                        // Step 3: Create Password
                        // =====================
                        try
                        {
                            // Wait for Create Password indicators
                            var pwdHeader = page.GetByText("Create Password");
                            if (await pwdHeader.CountAsync() > 0 || await page.GetByLabel("Password *").CountAsync() > 0)
                            {
                                Console.WriteLine("✅ Reached Create Password step. Setting password...");

                                var pwd = _opts.Password ?? "";
                                var pwdField = page.GetByLabel("Password *").First;
                                if (await pwdField.CountAsync() == 0)
                                    pwdField = page.Locator("input[type='password']").First;
                                await pwdField.FillAsync(pwd);

                                var confirmPwdField = page.GetByLabel("Confirm Password *").First;
                                if (await confirmPwdField.CountAsync() == 0)
                                    confirmPwdField = page.Locator("input[type='password']").Nth(1);
                                await confirmPwdField.FillAsync(pwd);

                                // Click DONE
                                var doneBtn = page.GetByRole(AriaRole.Button, new() { Name = "DONE" });
                                if (await doneBtn.CountAsync() == 0)
                                    doneBtn = page.GetByText("DONE");
                                if (await doneBtn.CountAsync() == 0)
                                    doneBtn = page.Locator("button:has-text('DONE')");

                                if (await doneBtn.CountAsync() > 0)
                                {
                                    var enabled = await doneBtn.IsEnabledAsync();
                                    if (!enabled)
                                    {
                                        // Nudge fields to trigger validation
                                        await confirmPwdField.PressAsync("Tab");
                                        await page.WaitForTimeoutAsync(300);
                                    }
                                    await doneBtn.ClickAsync();
                                    await page.WaitForTimeoutAsync(2000);
                                }

                                // Success indications
                                if (await page.GetByText("Send Money").CountAsync() > 0 || await page.GetByRole(AriaRole.Link, new() { Name = "Log Out" }).CountAsync() > 0)
                                {
                                    Console.WriteLine("🎉 Account created and logged in.");
                                    return true;
                                }
                            }
                        }
                        catch (Exception step3Ex)
                        {
                            Console.WriteLine("⚠️ Create Password step encountered an issue:");
                            Console.WriteLine(step3Ex);
                        }
                    }
                    catch (Exception step2Ex)
                    {
                        Console.WriteLine("⚠️ Billing Address step encountered an issue:");
                        Console.WriteLine(step2Ex);
                    }

                    return true;
                }
                else
                {
                    Console.WriteLine("❌ NEXT button not found");
                    return false;
                }
            }
            else
            {
                Console.WriteLine("❌ Sign Up button not found on login page.");
                Console.WriteLine("🔍 Debug: Checking page content for signup options...");

                // Debug: Check what's actually on the page
                var pageTitle = await page.TitleAsync();
                var currentUrl = page.Url;
                Console.WriteLine($"Current page title: {pageTitle}");
                Console.WriteLine($"Current URL: {currentUrl}");

                // Look for any text containing "sign" or "up" (case insensitive)
                var signUpText = page.Locator("text=/sign.?up/i");
                var count = await signUpText.CountAsync();
                Console.WriteLine($"Found {count} elements containing 'sign up' text");

                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error during login/signup:");
            Console.WriteLine(ex);
            return false;
        }
    }















    private async Task SignupAsync(IPage page)
    {

        await page.Locator("text=Sign Up, a:has-text('Sign Up')").First.ClickAsync();

        await page.FillAsync("input[name=FirstName]", _opts.FirstName);
        await page.FillAsync("input[name=LastName]", _opts.LastName);
        await page.FillAsync("input[name=EmailAddress]", _opts.Username);
        await page.FillAsync("input[name=Phone]", _opts.Phone);
        await page.FillAsync("input[name=Password]", _opts.Password);
        await page.FillAsync("input[name=ConfirmPassword]", _opts.Password);

        var checkbox = page.Locator("input[type=checkbox]");
        if (await checkbox.CountAsync() > 0)
            await checkbox.First.CheckAsync();

        await page.Locator("button[type=submit], input[type=submit]").ClickAsync();
        await page.WaitForTimeoutAsync(4000);

        Console.WriteLine("Sign-up completed (manual email verification may still be needed).");

    }

    public async Task PreparePaymentAsync(IPage page, string inmateCid, string inmateName)
    {
        try
        {
            await page.GotoAsync("https://www.accesscorrections.com/");
            // Navigate to Send Money from any page
            Console.WriteLine("➡️ Navigating to Send Money page...");
            ILocator sendMoneyBtn = page.GetByRole(AriaRole.Link, new() { Name = "Send Money" });
            if (await sendMoneyBtn.CountAsync() == 0)
                sendMoneyBtn = page.GetByRole(AriaRole.Button, new() { Name = "Send Money" });
            if (await sendMoneyBtn.CountAsync() == 0)
                sendMoneyBtn = page.Locator("a:has-text('Send Money'), button:has-text('Send Money')");

            if (await sendMoneyBtn.CountAsync() > 0)
            {
                await sendMoneyBtn.First.ClickAsync();
                // Wait for any of the Send Money controls to appear instead of a full load state
                try
                {
                    await page.WaitForSelectorAsync("text=Send Money, select, [placeholder*='Enter ID or Name'], [role='combobox']", new() { Timeout = 10000 });
                }
                catch { }
            }
            else
            {
                Console.WriteLine("❌ SEND MONEY button not found. Cannot proceed.");
                return;
            }

            // Step 1: Select State
            Console.WriteLine("🗺 Selecting state...");
            ILocator stateSelect = page.GetByLabel("State").First;
            if (await stateSelect.CountAsync() == 0)
                stateSelect = page.Locator("select:has(option), [aria-label*='State'], [role='combobox']").First;
            var desiredState = _opts.State ?? "Texas";
            if (await stateSelect.CountAsync() > 0)
            {
                var tagName = await stateSelect.EvaluateAsync<string>("el => el.tagName.toLowerCase()");
                if (tagName == "select")
                {
                    try { await stateSelect.SelectOptionAsync(new SelectOptionValue { Label = desiredState }); }
                    catch { await stateSelect.SelectOptionAsync(new[] { desiredState }); }
                }
                else
                {
                    // MUI Autocomplete / input with listbox
                    await stateSelect.ClickAsync();
                    await stateSelect.FillAsync("");
                    await stateSelect.TypeAsync(desiredState);
                    var option = page.GetByRole(AriaRole.Option, new() { Name = desiredState });
                    if (await option.CountAsync() == 0)
                        option = page.Locator($"li:has-text('{desiredState}'), div[role='option']:has-text('{desiredState}')").First;
                    if (await option.CountAsync() > 0)
                    {
                        await option.ClickAsync();
                    }
                    else
                    {
                        await stateSelect.PressAsync("Enter");
                    }
                }
            }
            await page.WaitForTimeoutAsync(800);

            // Step 2: Select Agency/Fund/Facility
            Console.WriteLine("🏢 Selecting agency...");
            ILocator agencySelect = page.GetByLabel("Agency").First;
            if (await agencySelect.CountAsync() == 0)
                agencySelect = page.Locator("select:has(option), [aria-label*='Agency'], [role='combobox']").Nth(1);
            if (await agencySelect.CountAsync() > 0)
            {
                var agencyLabel = string.IsNullOrEmpty(_opts.Agency) ? "Tarrant County Jail" : _opts.Agency;
                var tagA = await agencySelect.EvaluateAsync<string>("el => el.tagName.toLowerCase()");
                if (tagA == "select")
                {
                    try { await agencySelect.SelectOptionAsync(new SelectOptionValue { Label = agencyLabel }); }
                    catch { await agencySelect.SelectOptionAsync(new[] { agencyLabel }); }
                }
                else
                {
                    await agencySelect.ClickAsync();
                    await agencySelect.TypeAsync(agencyLabel);
                    var agencyOption = page.GetByRole(AriaRole.Option, new() { Name = agencyLabel });
                    if (await agencyOption.CountAsync() == 0)
                        agencyOption = page.Locator($"li:has-text('{agencyLabel}')").First;
                    if (await agencyOption.CountAsync() > 0)
                        await agencyOption.ClickAsync();
                    else
                        await agencySelect.PressAsync("Enter");
                }
            }
            await page.WaitForTimeoutAsync(800);

            // Step 3: Search inmate (CID first; fallback to Name)
            Console.WriteLine("🔎 Searching inmate (CID then Name)...");
            var step3Input = page.Locator("input[placeholder*='Enter ID or Name'], input[placeholder='Enter ID or Name (Last and First)']").First;
            if (await step3Input.CountAsync() == 0)
                step3Input = page.GetByRole(AriaRole.Textbox, new() { NameRegex = new System.Text.RegularExpressions.Regex("Enter ID or Name", System.Text.RegularExpressions.RegexOptions.IgnoreCase) }).First;

            async Task<bool> DoSearchAsync(string query)
            {
                if (string.IsNullOrWhiteSpace(query) || await step3Input.CountAsync() == 0) return false;
                await step3Input.FillAsync("");
                await step3Input.TypeAsync(query);
                await step3Input.PressAsync("Enter");
                // Click Search if present
                var searchBtnInner = page.GetByRole(AriaRole.Button, new() { Name = "Search" });
                if (await searchBtnInner.CountAsync() == 0)
                    searchBtnInner = page.Locator("button:has-text('Search')");
                if (await searchBtnInner.CountAsync() > 0)
                    await searchBtnInner.ClickAsync();
                try { await page.WaitForSelectorAsync("tbody tr, table tr, ul li", new() { Timeout = 4000 }); } catch { }
                var anyRow = await page.Locator("tbody tr, table tr, ul li").CountAsync();
                return anyRow > 0;
            }

            var foundResults = false;
            if (!string.IsNullOrWhiteSpace(inmateCid))
            {
                foundResults = await DoSearchAsync(inmateCid);
            }
            if (!foundResults && !string.IsNullOrWhiteSpace(inmateName))
            {
                // Try full name, then last name, then first name
                foundResults = await DoSearchAsync(inmateName);
                if (!foundResults)
                {
                    var parts = inmateName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 0)
                        foundResults = await DoSearchAsync(parts[0]);
                    if (!foundResults && parts.Length > 1)
                        foundResults = await DoSearchAsync(parts[1]);
                }
            }
            if (!foundResults)
            {
                Console.WriteLine("❌ No results after CID/Name searches");
            }

            // Step 4: Select inmate by Name (and CID if provided) from results
            Console.WriteLine("✅ Selecting inmate from results...");
            ILocator resultRow = null;
            if (!string.IsNullOrWhiteSpace(inmateName))
            {
                resultRow = page.Locator($"tr:has-text('{inmateName}')").First;
                if (await resultRow.CountAsync() == 0)
                {
                    // Try matching last name only (first token of input often last name on site)
                    var tokens = inmateName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (tokens.Length > 0)
                    {
                        var last = tokens[0];
                        resultRow = page.Locator($"tr:has-text('{last}')").First;
                    }
                }
            }
            if ((resultRow == null || await resultRow.CountAsync() == 0) && !string.IsNullOrWhiteSpace(inmateCid))
            {
                resultRow = page.Locator($"tr:has(td:has-text('{inmateCid}'))").First;
                if (await resultRow.CountAsync() == 0)
                    resultRow = page.Locator($"tr:has-text('{inmateCid}')").First;
            }
            if (resultRow == null || await resultRow.CountAsync() == 0)
            {
                // Fall back to the first data row if present
                resultRow = page.Locator("tbody tr").First;
                if (await resultRow.CountAsync() == 0)
                    resultRow = page.Locator("tr").Nth(1);
            }

            if (await resultRow.CountAsync() > 0)
            {
                // Prefer a Select/Continue button inside the row
                var selectBtn = resultRow.GetByRole(AriaRole.Button, new() { NameRegex = new System.Text.RegularExpressions.Regex("^(select|continue|next|add)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase) });
                if (await selectBtn.CountAsync() == 0)
                    selectBtn = resultRow.Locator("button:has-text('Select'), button:has-text('Continue'), button:has-text('Next'), a:has-text('Select'), a:has-text('Continue'), a:has-text('Next')").First;

                if (await selectBtn.CountAsync() > 0)
                {
                    await selectBtn.ClickAsync();
                }
                else
                {
                    await resultRow.ClickAsync();
                }

                await page.WaitForTimeoutAsync(1500);
                // If a Next button outside the row is required, try clicking it
                var nextOutside = page.GetByRole(AriaRole.Button, new() { Name = "Next" });
                if (await nextOutside.CountAsync() == 0)
                    nextOutside = page.Locator("button:has-text('Next'), a:has-text('Next')");
                if (await nextOutside.CountAsync() > 0)
                    await nextOutside.ClickAsync();
            }
            else
            {
                Console.WriteLine($"❌ Could not find inmate with CID {inmateCid} in results");
            }

            Console.WriteLine($"✅ Payment page prepared for inmate {inmateName} (CID {inmateCid}).");
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error preparing payment:");
            Console.WriteLine(ex.ToString());
        }
    }

    public class InmateSearchResult
    {
        public string State { get; set; } = string.Empty;
        public string Agency { get; set; } = string.Empty;
        public string InmateName { get; set; } = string.Empty;
        public string InmateId { get; set; } = string.Empty;
    }

    public async Task<List<InmateSearchResult>> FindInmateByNameAsync(IPage page, string inmateName, int maxResults = 10)
    {
        var results = new List<InmateSearchResult>();
        try
        {
            if (string.IsNullOrWhiteSpace(inmateName)) return results;

            // Go to Send Money page
            await page.GotoAsync("https://www.accesscorrections.com/v2/send-money");
            try { await page.WaitForSelectorAsync("select, [placeholder*='Enter ID or Name']", new() { Timeout = 8000 }); } catch { }

            // Locate selects
            var stateSelect = page.Locator("select").First;
            var agencySelect = page.Locator("select").Nth(1);
            if (await stateSelect.CountAsync() == 0)
            {
                Console.WriteLine("❌ Could not find state selector on Send Money page.");
                return results;
            }

            // Get state options
            var stateOptions = await stateSelect.Locator("option").AllInnerTextsAsync();
            foreach (var state in stateOptions)
            {
                var trimmedState = state?.Trim();
                if (string.IsNullOrWhiteSpace(trimmedState) || trimmedState.StartsWith("Select", StringComparison.OrdinalIgnoreCase))
                    continue;

                await stateSelect.SelectOptionAsync(new SelectOptionValue { Label = trimmedState });
                await page.WaitForTimeoutAsync(400);

                if (await agencySelect.CountAsync() == 0) agencySelect = page.Locator("select").Nth(1);
                var agencyOptions = await agencySelect.Locator("option").AllInnerTextsAsync();
                foreach (var agency in agencyOptions)
                {
                    var trimmedAgency = agency?.Trim();
                    if (string.IsNullOrWhiteSpace(trimmedAgency) || trimmedAgency.StartsWith("Select", StringComparison.OrdinalIgnoreCase))
                        continue;

                    await agencySelect.SelectOptionAsync(new SelectOptionValue { Label = trimmedAgency });
                    await page.WaitForTimeoutAsync(400);

                    // Step 3 search input
                    var step3Input = page.Locator("input[placeholder*='Enter ID or Name'], input[placeholder='Enter ID or Name (Last and First)']").First;
                    if (await step3Input.CountAsync() == 0)
                        step3Input = page.GetByRole(AriaRole.Textbox, new() { NameRegex = new System.Text.RegularExpressions.Regex("Enter ID or Name", System.Text.RegularExpressions.RegexOptions.IgnoreCase) }).First;
                    if (await step3Input.CountAsync() == 0)
                        continue;

                    await step3Input.FillAsync("");
                    await step3Input.TypeAsync(inmateName);
                    await step3Input.PressAsync("Enter");

                    var searchBtn = page.GetByRole(AriaRole.Button, new() { Name = "Search" });
                    if (await searchBtn.CountAsync() == 0)
                        searchBtn = page.Locator("button:has-text('Search')");
                    if (await searchBtn.CountAsync() > 0)
                        await searchBtn.ClickAsync();

                    try { await page.WaitForSelectorAsync("tbody tr, table tr", new() { Timeout = 4000 }); } catch { }

                    var rows = page.Locator("tbody tr, table tr");
                    var rowCount = await rows.CountAsync();
                    for (int i = 0; i < rowCount && results.Count < maxResults; i++)
                    {
                        var row = rows.Nth(i);
                        var rowText = await row.InnerTextAsync();
                        if (!rowText.Contains(inmateName, StringComparison.OrdinalIgnoreCase))
                        {
                            // Try with last name only
                            var parts = inmateName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length == 0 || !rowText.Contains(parts[0], StringComparison.OrdinalIgnoreCase))
                                continue;
                        }

                        // Extract a probable ID from any numeric cell/text
                        string inmateId = string.Empty;
                        try
                        {
                            var cells = await row.Locator("td").AllInnerTextsAsync();
                            foreach (var cell in cells)
                            {
                                var digits = new string(cell.Where(char.IsDigit).ToArray());
                                if (digits.Length >= 5) { inmateId = digits; break; }
                            }
                        }
                        catch { }

                        results.Add(new InmateSearchResult
                        {
                            State = trimmedState,
                            Agency = trimmedAgency,
                            InmateName = inmateName,
                            InmateId = inmateId
                        });
                    }

                    if (results.Count >= maxResults)
                        return results;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error during inmate discovery by name:");
            Console.WriteLine(ex);
        }
        return results;
    }


    //public async Task PreparePaymentAsync(IPage page, string inmateCid, string inmateName)
    //{
    //    try
    //    {
    //        await page.Locator("text=Send Money, a:has-text('Send Money')").First.ClickAsync();

    //        var input = page.Locator("input[name*=Inmate], input[placeholder*=Inmate]");
    //        await input.FillAsync(inmateCid ?? inmateName);

    //        await page.Locator("button[type=submit], input[type=submit]").First.ClickAsync();
    //        await page.WaitForTimeoutAsync(4000);

    //        Console.WriteLine($"✅ Payment page prepared for inmate {inmateName} (CID {inmateCid}).");
    //        Console.WriteLine("⚠️ Stopping before actual transaction.");
    //    }catch (Exception ex)
    //    {
    //        Console.WriteLine(ex.ToString());
    //    }
    //}
}
