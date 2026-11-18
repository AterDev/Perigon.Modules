using ApiTest.Data;
using AuditMod.Models.AuditFlowDtos;
using AuditMod.Models.AuditRecordDtos;
using AuditMod.Models.AuditTaskDtos;
using Entity.AuditMod;
using System.Net;
using System.Net.Http.Json;

namespace ApiTest;

public sealed class AuditModApiTests
{
    [ClassDataSource<TestHttpClientData>(Shared = SharedType.None)]
    [Test]
    public async Task AuditFlow_ShouldSubmitProgressAnyAndAllSteps(TestHttpClientData data)
    {
        var applicant = await data.CreateSysUserClientAsync();
        var auditorA = await data.CreateSysUserClientAsync();
        var auditorB = await data.CreateSysUserClientAsync();
        var outsider = await data.CreateSysUserClientAsync();
        string scene = $"test.audit.{Guid.NewGuid():N}";
        Guid flowId = Guid.Empty;

        try
        {
            AuditFlowDetailDto flow = await PostAsync<AuditFlowDetailDto>(data.HttpClient, "/api/admin/audit-flows", new AuditFlowAddDto
            {
                FlowKey = $"audit-{Guid.NewGuid():N}",
                SceneCode = scene,
                Name = "API audit flow",
                IsEnabled = true,
                Steps =
                [
                    new AuditFlowStepDto { StepNo = 1, Name = "Any approval", AuditMode = AuditMode.Any, AuditorUserIds = [auditorA.UserId, auditorB.UserId] },
                    new AuditFlowStepDto { StepNo = 2, Name = "All approval", AuditMode = AuditMode.All, AuditorUserIds = [auditorA.UserId, auditorB.UserId] }
                ]
            }, HttpStatusCode.Created);
            flowId = flow.Id;

            AuditRecordDetailDto record = await PostAsync<AuditRecordDetailDto>(applicant.Client, "/api/audit-records", new AuditRecordSubmitDto
            {
                SceneCode = scene,
                BizId = Guid.NewGuid().ToString("N"),
                ApplyReason = "API behavior test"
            }, HttpStatusCode.Created);
            await Assert.That(record.Status).IsEqualTo(Entity.AuditMod.AuditRecordStatus.InProgress);
            await Assert.That(record.Steps[0].Tasks.Count).IsEqualTo(2);

            Guid firstTask = record.Steps[0].Tasks.Single(task => task.AssigneeUserId == auditorA.UserId).Id;
            using (HttpResponseMessage outsiderResponse = await outsider.Client.PostAsJsonAsync($"/api/audit-tasks/{firstTask}/approve", new AuditDecisionDto { Comment = "not assigned" }))
            {
                await Assert.That(outsiderResponse.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);
            }

            AuditTaskDetailDto firstDecision = await PostAsync<AuditTaskDetailDto>(auditorA.Client, $"/api/audit-tasks/{firstTask}/approve", new AuditDecisionDto { Comment = "any accepted" }, HttpStatusCode.OK);
            await Assert.That(firstDecision.Record.Status).IsEqualTo(Entity.AuditMod.AuditRecordStatus.InProgress);
            await Assert.That(firstDecision.Record.CurrentStepNo).IsEqualTo(2);

            AuditRecordDetailDto progressed = await GetAsync<AuditRecordDetailDto>(applicant.Client, $"/api/audit-records/{record.Id}", HttpStatusCode.OK);
            Guid secondTaskA = progressed.Steps[1].Tasks.Single(task => task.AssigneeUserId == auditorA.UserId).Id;
            Guid secondTaskB = progressed.Steps[1].Tasks.Single(task => task.AssigneeUserId == auditorB.UserId).Id;
            Task<HttpResponseMessage> approveA = auditorA.Client.PostAsJsonAsync($"/api/audit-tasks/{secondTaskA}/approve", new AuditDecisionDto { Comment = "all A" });
            Task<HttpResponseMessage> approveB = auditorB.Client.PostAsJsonAsync($"/api/audit-tasks/{secondTaskB}/approve", new AuditDecisionDto { Comment = "all B" });
            HttpResponseMessage[] decisions = await Task.WhenAll(approveA, approveB);
            try
            {
                await Assert.That(decisions.All(response => response.StatusCode == HttpStatusCode.OK)).IsTrue();
            }
            finally
            {
                foreach (HttpResponseMessage response in decisions) response.Dispose();
            }

            AuditRecordDetailDto completed = await GetAsync<AuditRecordDetailDto>(applicant.Client, $"/api/audit-records/{record.Id}", HttpStatusCode.OK);
            await Assert.That(completed.Status).IsEqualTo(Entity.AuditMod.AuditRecordStatus.Approved);
        }
        finally
        {
            if (flowId != Guid.Empty)
            {
                using HttpResponseMessage response = await data.HttpClient.DeleteAsync($"/api/admin/audit-flows/{flowId}");
            }
        }
    }

    [ClassDataSource<TestHttpClientData>(Shared = SharedType.None)]
    [Test]
    public async Task AuditRecord_ShouldRejectDuplicateSubmissionAndSupportWithdraw(TestHttpClientData data)
    {
        var applicant = await data.CreateSysUserClientAsync();
        var auditor = await data.CreateSysUserClientAsync();
        string scene = $"test.audit.withdraw.{Guid.NewGuid():N}";
        Guid flowId = Guid.Empty;
        string bizId = Guid.NewGuid().ToString("N");

        try
        {
            AuditFlowDetailDto flow = await PostAsync<AuditFlowDetailDto>(data.HttpClient, "/api/admin/audit-flows", new AuditFlowAddDto
            {
                FlowKey = $"audit-withdraw-{Guid.NewGuid():N}", SceneCode = scene, Name = "Withdraw flow", IsEnabled = true,
                Steps = [new AuditFlowStepDto { StepNo = 1, Name = "Review", AuditMode = AuditMode.Any, AuditorUserIds = [auditor.UserId] }]
            }, HttpStatusCode.Created);
            flowId = flow.Id;

            AuditRecordDetailDto record = await PostAsync<AuditRecordDetailDto>(applicant.Client, "/api/audit-records", new AuditRecordSubmitDto { SceneCode = scene, BizId = bizId, ApplyReason = "duplicate test" }, HttpStatusCode.Created);
            using (HttpResponseMessage duplicate = await applicant.Client.PostAsJsonAsync("/api/audit-records", new AuditRecordSubmitDto { SceneCode = scene, BizId = bizId, ApplyReason = "duplicate" }))
            {
                await Assert.That(duplicate.StatusCode).IsEqualTo(HttpStatusCode.Conflict);
            }

            AuditRecordDetailDto withdrawn = await PostAsync<AuditRecordDetailDto>(applicant.Client, $"/api/audit-records/{record.Id}/withdraw", new AuditRecordWithdrawDto { Reason = "cancelled by applicant" }, HttpStatusCode.OK);
            await Assert.That(withdrawn.Status).IsEqualTo(Entity.AuditMod.AuditRecordStatus.Withdrawn);
            using (HttpResponseMessage secondWithdraw = await applicant.Client.PostAsJsonAsync($"/api/audit-records/{record.Id}/withdraw", new AuditRecordWithdrawDto { Reason = "duplicate withdraw" }))
            {
                await Assert.That(secondWithdraw.StatusCode).IsEqualTo(HttpStatusCode.Conflict);
            }
        }
        finally
        {
            if (flowId != Guid.Empty)
            {
                using HttpResponseMessage response = await data.HttpClient.DeleteAsync($"/api/admin/audit-flows/{flowId}");
            }
        }
    }

    private static async Task<T> PostAsync<T>(HttpClient client, string path, object body, HttpStatusCode expected)
    {
        using HttpResponseMessage response = await client.PostAsJsonAsync(path, body);
        await Assert.That(response.StatusCode).IsEqualTo(expected);
        return await response.Content.ReadFromJsonAsync<T>() ?? throw new InvalidOperationException($"{path} returned an empty body.");
    }

    private static async Task<T> GetAsync<T>(HttpClient client, string path, HttpStatusCode expected)
    {
        using HttpResponseMessage response = await client.GetAsync(path);
        await Assert.That(response.StatusCode).IsEqualTo(expected);
        return await response.Content.ReadFromJsonAsync<T>() ?? throw new InvalidOperationException($"{path} returned an empty body.");
    }
}
