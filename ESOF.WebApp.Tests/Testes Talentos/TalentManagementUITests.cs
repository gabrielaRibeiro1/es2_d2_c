using System;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Xunit;
using Assert = Xunit.Assert;

//comment
namespace ESOF.WebApp.Tests
{
    public class TalentManagementUITests : IDisposable
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public TalentManagementUITests()
        {
            var options = new ChromeOptions();
            _driver = new ChromeDriver(options);
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));
        }

        [Fact]
        public void ShouldCreateNewTalentProfile()
        {
            _driver.Navigate().GoToUrl("http://localhost:5297/talent-management");

            _wait.Until(d => d.FindElement(By.CssSelector("form")));

            FillInput("input[placeholder='Nome do Perfil *']", "Teste Selenium");
            FillInput("input[placeholder='País *']", "Portugal");
            FillInput("input[placeholder='Email *']", "teste@teste.com");
            FillInput("input[placeholder='Preço *']", "100");
            FillInput("input[placeholder='Categoria']", "TI");
            FillInput("input[placeholder='ID (FK) *']", "3");

            _wait.Until(driver =>
            {
                try
                {
                    var select = new SelectElement(driver.FindElement(By.TagName("select")));
                    select.SelectByValue("0");
                    return true;
                }
                catch (StaleElementReferenceException)
                {
                    return false;
                }
            });

            ClickSubmitButton();

            var successMsg = _wait.Until(d => d.FindElement(By.ClassName("alert-success")));
            Assert.Contains("Perfil criado com sucesso", successMsg.Text);
        }

        [Fact]
        public void ShouldUpdateTalentProfile()
        {
            _driver.Navigate().GoToUrl("http://localhost:5297/talent-management");

            _wait.Until(d => d.FindElements(By.XPath("//table//tr")).Count > 1);
            
            var editButton = _wait.Until(d => d.FindElement(By.XPath("//button[contains(text(),'Editar')]")));
            editButton.Click();

            _wait.Until(d => d.FindElement(By.CssSelector("form")));
            
            FillInput("input[placeholder='Preço *']", "250");

            ClickSubmitButton();
            
            var successMsg = _wait.Until(d => d.FindElement(By.ClassName("alert-success")));
            Assert.Contains("Perfil atualizado com sucesso", successMsg.Text);
        }


        [Fact]
        public void ShouldDeleteTalentProfile()
        {
            _driver.Navigate().GoToUrl("http://localhost:5297/talent-management");

            _wait.Until(d => d.FindElements(By.XPath("//table//tr")).Count > 1);

            var deleteButton = _wait.Until(d => d.FindElement(By.XPath("//button[contains(text(),'Excluir')]")));
            deleteButton.Click();
            
            try
            {
                IAlert alert = _wait.Until(ExpectedConditions.AlertIsPresent());
                alert.Accept();
            }
            catch (WebDriverTimeoutException)
            {
                Assert.True(false, "Alerta de confirmação não apareceu ao tentar excluir.");
            }

            var successMsg = _wait.Until(d => d.FindElement(By.ClassName("alert-success")));
            Assert.Contains("Perfil excluído com sucesso", successMsg.Text);
        }

        private void FillInput(string cssSelector, string value)
        {
            _wait.Until(driver =>
            {
                try
                {
                    var input = driver.FindElement(By.CssSelector(cssSelector));
                    input.Clear();
                    input.SendKeys(value);
                    return true;
                }
                catch (StaleElementReferenceException)
                {
                    return false;
                }
            });
        }

        private void ClickSubmitButton()
        {
            _wait.Until(driver =>
            {
                try
                {
                    var button = driver.FindElement(By.CssSelector("button[type='submit']"));
                    button.Click();
                    return true;
                }
                catch (StaleElementReferenceException)
                {
                    return false;
                }
            });
        }

        public void Dispose()
        {
            _driver.Quit();
        }
    }

    public static class ExpectedConditions
    {
        public static Func<IWebDriver, IAlert> AlertIsPresent()
        {
            return driver =>
            {
                try
                {
                    return driver.SwitchTo().Alert();
                }
                catch (NoAlertPresentException)
                {
                    return null;
                }
            };
        }
    }
}
