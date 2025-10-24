﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net.WebSockets;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Microsoft.Playwright;

using TNU_AutoClass.Helpers;

using static System.Collections.Specialized.BitVector32;
using static System.Windows.Forms.LinkLabel;

namespace TNU_AutoClass
{
    public partial class FrmMain : Form
    {

        public FrmMain()
        {
            InitializeComponent();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            txtTK.Text = Utils.GetConfigValue("UserName", "");
            txtMK.Text = Utils.GetConfigValue("Password", "");
        }

        private void txtTK_TextChanged(object sender, EventArgs e)
        {
            Utils.UpdateConfigValue("UserName", txtTK.Text.Trim());
        }

        private void txtMK_TextChanged(object sender, EventArgs e)
        {
            Utils.UpdateConfigValue("Password", txtMK.Text.Trim());
        }

        private async void btnBD_Click(object sender, EventArgs e)
        {
            await Start();
        }

        private async Task Start()
        {
            const string loginPath = "https://tnu.aum.edu.vn/login/index.php";

            var browser = (await new PlaywrightHelper().InitBrowserAsync()).Browser;
            if (browser == null) return;

            AddStatusText("Khởi tạo browser thành công!");

            var page = await browser.NewPageAsync();
            await page.GotoAsync(loginPath);

            AddStatusText($"Điều hướng đến {loginPath}");
            await Task.Run(() => Thread.Sleep(2000));

            await page.Locator("input#username[type='text'][name='username']").FillAsync(txtTK.Text.Trim());
            await page.Locator("input#password[type='password'][name='password']").FillAsync(txtMK.Text.Trim());
            await page.Locator("button#loginbtn[type='submit']").ClickAsync();

            await Task.Run(() => Thread.Sleep(2000));

            var failedClass = await page.QuerySelectorAsync("div.alert.alert-danger[role='alert']");
            if (failedClass != null)
            {
                AddStatusText("Đăng nhập sai, vui lòng thử lại");
                return;
            }

            AddStatusText($"Đăng nhập thành công tài khoản: {txtTK.Text.Trim()}");
            AddStatusText($"Bắt đầu lọc kết quả...");

            await page.Locator("a.dropdown-item[data-filter='grouping'][data-value='inprogress'][data-pref='inprogress']").DispatchEventAsync("click"); ;
            var courseDivs = await page.QuerySelectorAllAsync("div[data-region='courses-view'] a.aalink.coursename.mr-2");

            List<string> urls = new List<string>();
            foreach (var courseDiv in courseDivs)
            {
                var link = await courseDiv.GetAttributeAsync("href");
                if (string.IsNullOrEmpty(link)) continue;

                urls.Add(link);
            }

            AddStatusText($"Tìm thấy {urls.Count} khoá học");

            foreach (var url in urls)
            {
                AddStatusText($"Đi đến khoá học: {url}");
                await page.GotoAsync(url);
                await Task.Run(() => Thread.Sleep(2000));

                var forms = await page.QuerySelectorAllAsync("form[action='https://tnu.aum.edu.vn/course/togglecompletion.php']");
                AddStatusText($"Bắt đầu tìm kiếm tất cả checkbox bấm được");
                foreach (var form in forms)
                {
                    var completeStateInput = await form.QuerySelectorAsync("input[name='completionstate']");
                    if (completeStateInput == null) continue;

                    var completeStateInputValue = await completeStateInput.GetAttributeAsync("value");
                    if (string.IsNullOrEmpty(completeStateInputValue)) continue;
                    if (completeStateInputValue == "0") continue;

                    var button = await form.QuerySelectorAsync("button");
                    if (button == null) continue;

                    await button.ClickAsync();
                    await Task.Run(() => Thread.Sleep(2000));
                }
                AddStatusText("Đã bấm hết các checkbox...");

                AddStatusText("Bắt đầu truy cập các bài giảng điện tử...");
                var sections = await page.QuerySelectorAllAsync("li.activity.label.modtype_label table tr td a");
                Dictionary<string, string> sectionFragment = new Dictionary<string, string>();

                foreach (var section in sections)
                {
                    var sectionHref = await section.GetAttributeAsync("href");
                    if (string.IsNullOrEmpty(sectionHref)) continue;

                    var uri = new Uri(sectionHref);
                    var fragment = uri.Fragment;

                    if (fragment.StartsWith("#")) fragment = fragment.Substring(1);
                    if (string.IsNullOrEmpty(fragment)) continue;

                    var innerText = await section.InnerHTMLAsync();
                    if (string.IsNullOrEmpty(innerText)) continue;

                    sectionFragment[fragment] = innerText;
                }

                var sectionIndex = 1;
                foreach (var (key, value) in sectionFragment)
                {
                    var dates = value.Split("- ");
                    if (dates.Length != 2) continue;

                    var fromDate = DateTime.ParseExact(dates[0].Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    var today = DateTime.Now;

                    if (today < fromDate) continue;

                    AddStatusText($"Bắt đầu kiểm tra tuần: {sectionIndex}");

                    var checkComplete = await page.QuerySelectorAsync($"li#{key} img[src*='completion-auto-y']");
                    if (checkComplete != null) continue;

                    var scromAnchor = await page.QuerySelectorAsync($"li#{key} .activity.scorm.modtype_scorm a.aalink");
                    if (scromAnchor == null) continue;

                    var scromUrl = await scromAnchor.GetAttributeAsync("href");
                    if (string.IsNullOrEmpty(scromUrl)) continue;
                    await page.GotoAsync(scromUrl);

                    await Task.Run(() => Thread.Sleep(2000));

                    var popupTask = page.WaitForPopupAsync();
                    await page.Locator("form#scormviewform input[type='submit']").ClickAsync();

                    var popup = await popupTask;
                    await popup.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
                    await Task.Run(() => Thread.Sleep(2000));

                    AddStatusText($"Bắt đầu bài giảng điện tử của tuần: {sectionIndex}");

                    var slideIndex = 1;
                    while (1 == 1)
                    {
                        var frameElement = await popup.QuerySelectorAsync("iframe#scorm_object");
                        if (frameElement == null) break;

                        var frame = await frameElement.ContentFrameAsync();
                        if (frame == null) break;

                        var dialog = await frame.QuerySelectorAsync("div.message-box[role='alertdialog'] button:nth-of-type(2)");
                        if (dialog != null)
                        {
                            await dialog.ClickAsync();
                        }
                        await Task.Run(() => Thread.Sleep(2000));

                        var button = await frame.QuerySelectorAsync("button.universal-control-panel__button.universal-control-panel__button_next.universal-control-panel__button_right-arrow:not([disabled])");
                        if (button == null)
                        {
                            await popup.CloseAsync();
                            AddStatusText($"Đã skip toàn bộ slide!");
                            break;
                        }

                        await button.ClickAsync();
                        AddStatusText($"Đã skip slide: {slideIndex}");
                        await Task.Run(() => Thread.Sleep(500));
                        slideIndex++;
                    }
                    await Task.Run(() => Thread.Sleep(2000));
                    sectionIndex++;
                }
                AddStatusText("Đã hoàn thành việc truy cập các bài giảng điện tử!");

                AddStatusText("Bắt đầu truy cập các bài luyện tập");
                foreach (var (key, value) in sectionFragment)
                {
                    var dates = value.Split("- ");
                    if (dates.Length != 2) continue;

                    var fromDate = DateTime.ParseExact(dates[0].Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    var today = DateTime.Now;

                    if (today < fromDate) continue;

                    var anchor = await page.QuerySelectorAsync($"li#{key} .activity.quiz.modtype_quiz .aalink");
                    if (anchor == null) continue;

                    var accessHide = await page.QuerySelectorAsync($"li#{key} .activity.quiz.modtype_quiz .aalink .accesshide");
                    if (accessHide == null) continue;

                    var quizUrl = await anchor.GetAttributeAsync("href");
                    if (string.IsNullOrEmpty(quizUrl)) continue;

                    await page.GotoAsync(quizUrl);
                    await Task.Run(() => Thread.Sleep(2000));
                    AddStatusText($"Đi đến bài luyện tập: {quizUrl}");
                    AddStatusText($"Kiểm tra bài đã max điểm hay chưa");

                    var pointCells = await page.QuerySelectorAllAsync("table.generaltable.quizattemptsummary tr .cell.c3");
                    var breakLoop = false;
                    foreach (var cell in pointCells)
                    {
                        var point = (int)double.Parse(await cell.InnerTextAsync(), CultureInfo.GetCultureInfo("vi-VN"));
                        if (point == 10)
                        {
                            breakLoop = true;
                            continue;
                        }
                    }

                    if (breakLoop)
                    {
                        AddStatusText("Bài luyện tập đã max điểm, quay lại...");
                        await page.GotoAsync(url);
                        continue;
                    }

                    var totalCount = 1;
                    var breakQuizLoop = false;
                    var dictionaryQuestions = new Dictionary<string, List<string>>();
                    while (!breakQuizLoop)
                    {
                        AddStatusText($"Bắt đầu làm bài kiểm tra, lần thứ: {totalCount}");
                        await page.Locator("div.singlebutton.quizstartbuttondiv form button[type='submit']").ClickAsync();
                        await Task.Run(() => Thread.Sleep(2000));

                        await page.Locator("div#fgroup_id_buttonar fieldset input[type='submit'].btn.btn-primary").ClickAsync();
                        await Task.Run(() => Thread.Sleep(2000));

                        if (totalCount == 1)
                        {
                            AddStatusText("Skip lần đầu để kiểm tra câu trả lời");
                            await page.Locator("a.endtestlink.aalink").ClickAsync();
                            await Task.Run(() => Thread.Sleep(2000));

                            await page.Locator("div.singlebutton form[action='https://tnu.aum.edu.vn/mod/quiz/processattempt.php'] button").ClickAsync();
                            await Task.Run(() => Thread.Sleep(2000));

                            await page.Locator("div.moodle-dialogue-base.moodle-dialogue-confirm input.btn.btn-primary").ClickAsync();
                            await Task.Run(() => Thread.Sleep(2000));

                            var questions = await page.QuerySelectorAllAsync("div.qtext");
                            var answers = await page.QuerySelectorAllAsync("div.rightanswer p.cell");

                            if (questions.Count == answers.Count)
                            {
                                for (int i = 0; i < questions.Count; i++)
                                {
                                    var question = (await questions[i].InnerTextAsync()).Trim();
                                    var answer = (await answers[i].InnerTextAsync()).Trim();

                                    if (dictionaryQuestions.ContainsKey(question))
                                    {
                                        dictionaryQuestions[question].Add(answer);
                                    }
                                    else
                                    {
                                        dictionaryQuestions[question] = new List<string> { answer };
                                    }
                                }

                            }
                            await page.Locator("div.othernav a.mod_quiz-next-nav").ClickAsync();
                            await Task.Run(() => Thread.Sleep(2000));
                        }
                        else
                        {
                            AddStatusText("Bắt đầu lần thử");
                            while (true)
                            {
                                var nextButton = await page.QuerySelectorAsync("form[action^='https://tnu.aum.edu.vn/mod/quiz/processattempt.php'] input[type='submit'][name='next']");
                                var questionDivs = await page.QuerySelectorAllAsync("div[id^='question-']");
                                foreach (var questionDiv in questionDivs)
                                {
                                    var questionElement = await questionDiv.QuerySelectorAsync("div.qtext");
                                    if (questionElement == null) continue;

                                    var question = (await questionElement.InnerTextAsync()).Trim();
                                    var answerElements = await questionDiv.QuerySelectorAllAsync("div.answer div[class^='r'] p.cell");
                                    var rand = new Random().Next(4);
                                    for (int i = 0; i < answerElements.Count; i++)
                                    {
                                        var answer = (await answerElements[i].InnerTextAsync()).Trim();
                                        await answerElements[rand].ClickAsync();

                                        if (dictionaryQuestions.ContainsKey(question))
                                        {
                                            var item = dictionaryQuestions[question];
                                            if (item.Contains(answer))
                                            {
                                                await answerElements[i].ClickAsync();
                                                await Task.Run(() => Thread.Sleep(2000));
                                                break;
                                            }
                                        } 
                                    }
                                    
                                }

                                if (nextButton == null)
                                {
                                    await page.Locator("div.singlebutton form[action='https://tnu.aum.edu.vn/mod/quiz/processattempt.php'] button").ClickAsync();
                                    await Task.Run(() => Thread.Sleep(2000));

                                    await page.Locator("div.moodle-dialogue-base.moodle-dialogue-confirm input.btn.btn-primary").ClickAsync();
                                    await Task.Run(() => Thread.Sleep(2000));

                                    AddStatusText("Đã trả lời xong");
                                    var incorrectAnsDivs = await page.QuerySelectorAllAsync("div[id^='question-'].incorrect");
                                    if (incorrectAnsDivs.Count() <= 0)
                                    {
                                        breakQuizLoop = true;
                                        breakLoop = true;
                                    }
                                    else
                                    {
                                        foreach (var incorrectAnsDiv in incorrectAnsDivs)
                                        {
                                            var questionDiv = await incorrectAnsDiv.QuerySelectorAsync("div.qtext");
                                            if (questionDiv == null) continue;

                                            var rightAnsDiv = await incorrectAnsDiv.QuerySelectorAsync("div.rightanswer p.cell");
                                            if (rightAnsDiv == null) continue;

                                            var question = (await questionDiv.InnerTextAsync()).Trim();
                                            if (string.IsNullOrEmpty(question)) continue;

                                            var answer = (await rightAnsDiv.InnerTextAsync()).Trim();
                                            if (string.IsNullOrEmpty(answer)) continue;

                                            if (dictionaryQuestions.ContainsKey(question))
                                            {
                                                dictionaryQuestions[question].Add(answer);
                                            }
                                            else
                                            {
                                                dictionaryQuestions[question] = new List<string> { answer };
                                            }
                                        }
                                    }

                                    await page.Locator("div.othernav a.mod_quiz-next-nav").ClickAsync();
                                    await Task.Run(() => Thread.Sleep(2000));
                                    break;
                                }
                                await nextButton.ClickAsync();
                                await Task.Run(() => Thread.Sleep(2000));
                            }
                        }
                        totalCount++;
                    }
                    await page.GotoAsync(url);
                    await Task.Run(() => Thread.Sleep(2000));
                }
                AddStatusText("Đã hoàn thành việc truy cập các bài luyện tập");
            }
        }

        private void AddStatusText(string text, bool isClear = false)
        {
            if (isClear) rtbTrangThai.Clear();
            rtbTrangThai.Text += text + Environment.NewLine;
        }
    }
}
