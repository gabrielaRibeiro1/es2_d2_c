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
    public void Test_NavigateToWorkProposals_AfterLogin()
    {
        _driver!.Navigate().GoToUrl("http://localhost:5297/");
        Assert.That(_driver.Title, Does.Contain("Home"));

        Thread.Sleep(1000);
        
        _wait!.Until(d => d.FindElement(By.LinkText("Login"))).Click();
        
        Thread.Sleep(1000);

        _wait.Until(d => d.FindElement(By.Id("username"))).SendKeys("joao");
        _driver.FindElement(By.Id("password")).SendKeys("joao123");
        _driver.FindElement(By.CssSelector("button[type='submit']")).Click();
        
        Thread.Sleep(1000);

        _wait.Until(d => d.FindElement(By.XPath("//a[contains(text(),'Work Proposals')]"))).Click();

        _wait.Until(d => d.Url.Contains("work-proposal"));

        Assert.That(_driver.Url, Does.Contain("work-proposal"));
        
        Thread.Sleep(2000);
        
        _wait.Until(d => d.FindElement(By.CssSelector("input[required]")));
        
        _driver.FindElement(By.CssSelector("input[placeholder='Proposal Name'], input.form-control")).SendKeys("Test Proposal Selenium");
        _driver.FindElements(By.CssSelector("input.form-control"))[1].SendKeys("QA Testing");
        _driver.FindElements(By.CssSelector("input.form-control"))[2].SendKeys("Selenium, NUnit");
        _driver.FindElement(By.CssSelector("input[type='number']")).SendKeys("3");
        _driver.FindElement(By.TagName("textarea")).SendKeys("Automated test of proposal creation.");
        _driver.FindElements(By.CssSelector("input[type='number']")).Last().SendKeys("40");
        
        Thread.Sleep(2000);
        
        _driver.FindElement(By.CssSelector("button[type='submit']")).Click();

        var successMsg = _wait.Until(d => d.FindElement(By.XPath("//*[contains(text(),'Proposal created successfully!')]")));
        Assert.That(successMsg.Text, Contains.Substring("Proposal created successfully!"));
        
        Thread.Sleep(2000);
        
        _driver.Navigate().Refresh();
        
        Thread.Sleep(5000);
        
        Assert.That(_driver.PageSource, Contains.Substring("Test Proposal Selenium"));
    }

    [TearDown]
    public void Teardown()
    {
        _driver?.Quit();
    }
}