using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace Tests;

public class UiTests
{
    private IWebDriver? _driver;
    private WebDriverWait? _wait;

    [SetUp]
    public void Setup()
    {
        _driver = new ChromeDriver();
        _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
    }

    [Test]
    public void Test_Login_Navigate_CreateWorkProposal()
    {
        // Open the Frontend
        _driver!.Navigate().GoToUrl("http://localhost:5297/");
        Assert.That(_driver.Title, Does.Contain("Home"));

        Thread.Sleep(2000);
        
        // Navigate to the Login page
        _wait!.Until(d => d.FindElement(By.LinkText("Login"))).Click();
        
        Thread.Sleep(1000);

        // Fill out the login form
        _wait.Until(d => d.FindElement(By.Id("username"))).SendKeys("joao");
        _driver.FindElement(By.Id("password")).SendKeys("joao123");
        _driver.FindElement(By.CssSelector("button[type='submit']")).Click();
        
        Thread.Sleep(3000);

        // Navigate to the Work Proposals page
        _wait.Until(d => d.FindElement(By.XPath("//a[contains(text(),'Work Proposals')]"))).Click();

        // Check that we're in the correct URL
        _wait.Until(d => d.Url.Contains("work-proposal"));
        Assert.That(_driver.Url, Does.Contain("work-proposal"));
        
        Thread.Sleep(2000);
        
        // Fill out create work proposal form
        _wait.Until(d => d.FindElement(By.CssSelector("input[required]")));
        _driver.FindElement(By.CssSelector("input[placeholder='Proposal Name'], input.form-control")).SendKeys("Test Proposal Selenium");
        _driver.FindElements(By.CssSelector("input.form-control"))[1].SendKeys("QA Testing");
        _driver.FindElements(By.CssSelector("input.form-control"))[2].SendKeys("Selenium, NUnit");
        _driver.FindElement(By.CssSelector("input[type='number']")).SendKeys("3");
        _driver.FindElement(By.TagName("textarea")).SendKeys("Automated test of proposal creation.");
        _driver.FindElements(By.CssSelector("input[type='number']")).Last().SendKeys("40");
        
        Thread.Sleep(2000);
        
        // Click the create button
        _driver.FindElement(By.CssSelector("button[type='submit']")).Click();

        // Check for the success message
        var successMsg = _wait.Until(d => d.FindElement(By.XPath("//*[contains(text(),'Proposal created successfully!')]")));
        Assert.That(successMsg.Text, Contains.Substring("Proposal created successfully!"));
        
        // Refresh page for visual confirmation
        Thread.Sleep(2000);
        _driver.Navigate().Refresh();
        Thread.Sleep(4000);
        
        // Check if the new proposal with the given name is now present
        Assert.That(_driver.PageSource, Contains.Substring("Test Proposal Selenium"));
    }
    
    [Test]
    public void Test_Update_Delete_Proposal()
    {
        // Open the Frontend
        _driver!.Navigate().GoToUrl("http://localhost:5297/");
        
        Thread.Sleep(3000);

        // Navigate to Login page
        _wait!.Until(d => d.FindElement(By.LinkText("Login"))).Click();
        
        Thread.Sleep(1000);
        
        // Fill out login form and sign in
        _wait.Until(d => d.FindElement(By.Id("username"))).SendKeys("joao");
        _driver.FindElement(By.Id("password")).SendKeys("joao123");
        _driver.FindElement(By.CssSelector("button[type='submit']")).Click();

        Thread.Sleep(3000);

        // Navigate to Work Proposals page
        _wait.Until(d => d.FindElement(By.XPath("//a[contains(text(),'Work Proposals')]"))).Click();
        _wait.Until(d => d.Url.Contains("work-proposal"));

        Thread.Sleep(2000);

        // Fill out the create proposal form
        const string proposalName = "Selenium Proposal To Edit/Delete";
        _wait.Until(d => d.FindElement(By.CssSelector("input[required]")));
        _driver.FindElement(By.CssSelector("input[placeholder='Proposal Name'], input.form-control")).SendKeys(proposalName);
        _driver.FindElements(By.CssSelector("input.form-control"))[1].SendKeys("Initial Project");
        _driver.FindElements(By.CssSelector("input.form-control"))[2].SendKeys("Selenium");
        _driver.FindElement(By.CssSelector("input[type='number']")).SendKeys("5");
        _driver.FindElement(By.TagName("textarea")).SendKeys("Testing update and delete features.");
        _driver.FindElements(By.CssSelector("input[type='number']")).Last().SendKeys("30");

        Thread.Sleep(2000);
        _driver.FindElement(By.CssSelector("button[type='submit']")).Click();
        
        // Check for success message
        var successMsg = _wait.Until(d => d.FindElement(By.XPath("//*[contains(text(),'Proposal created successfully!')]")));
        Assert.That(successMsg.Text, Contains.Substring("Proposal created successfully!"));

        //Refresh page for visual confirmation
        Thread.Sleep(1000);
        _driver.Navigate().Refresh();
        Thread.Sleep(1000);

        // Search for the specific proposal it just created
        var row = _wait.Until(d => d.FindElements(By.CssSelector("table tbody tr"))
            .FirstOrDefault(r => r.Text.Contains("Selenium Proposal To Edit/Delete")));
        Assert.That(row, Is.Not.Null, "Row with proposal not found");

        // Click the Edit button on the correct row
        var editButton = row.FindElements(By.TagName("button"))
            .FirstOrDefault(b => b.Text.Contains("Edit"));
        Assert.That(editButton, Is.Not.Null, "Edit button not found");
        editButton?.Click();
        Thread.Sleep(2000);

        // Update the proposal's name
        var updatedName = "Updated Selenium Proposal";
        var nameInput = _wait.Until(d => d.FindElement(By.CssSelector("input[placeholder='Proposal Name']")));
        nameInput.Clear();
        nameInput.SendKeys(updatedName);
        
        Thread.Sleep(1000);

        // Click the save button
        _driver.FindElements(By.TagName("button")).FirstOrDefault(b => b.Text.Contains("Save"))?.Click();

        // Check for the success message
        var updateMsg = _wait.Until(d => d.FindElement(By.XPath("//*[contains(text(),'Proposal updated!')]")));
        Assert.That(updateMsg.Text, Contains.Substring("Proposal updated!"));

        // Refresh the page for visual confirmation
        Thread.Sleep(1000);
        _driver.Navigate().Refresh();
        Thread.Sleep(1000);

        // Check if the update proposal is present
        Assert.That(_driver.PageSource, Contains.Substring(updatedName));

        // Search for the updated proposal
        var updatedRow = _wait.Until(d => d.FindElements(By.CssSelector("tr"))
            .FirstOrDefault(tr => tr.Text.Contains(updatedName)));
        Assert.That(updatedRow, Is.Not.Null, "Updated proposal not found.");

        Thread.Sleep(1000);
        // Click the Delete button
        var deleteButton = updatedRow.FindElements(By.TagName("button")).FirstOrDefault(b => b.Text.Contains("Delete"));
        Assert.That(deleteButton, Is.Not.Null, "Delete button not found.");
        deleteButton!.Click();

        // Check for the success message
        var deleteMsg = _wait.Until(d => d.FindElement(By.XPath("//*[contains(text(),'Proposal deleted!')]")));
        Assert.That(deleteMsg.Text, Contains.Substring("Proposal deleted!"));

        // Refresh the page for visual confirmation
        Thread.Sleep(1000);
        _driver.Navigate().Refresh();
        Thread.Sleep(3000);

        // Check if the deleted proposal is no longer present
        Assert.That(_driver.PageSource, Does.Not.Contain(updatedName));
    }

    [TearDown]
    public void Teardown()
    {
        _driver?.Quit();
    }
}