using System;
using NUnit.Framework;
using UIForia.Systems;

[TestFixture]
public class TaskSystemTests {

    private class Task0 : UITask {

        public int variable;
        public string status;
        public int runCount;
        public int targetRunCount;
        public UITaskResult result;

        public Task0(int targetRunCount, UITaskResult result = UITaskResult.Completed) {
            this.targetRunCount = targetRunCount;
            this.result = result;
        }

        public override void OnInitialized() {
            variable = 1;
        }

        public override void OnCancelled() {
            status = "cancelled";
        }

        public override void OnFailed() {
            status = "failed";
        }

        public override UITaskResult Run(float deltaTime) {
            runCount++;
            return runCount == targetRunCount ? result : UITaskResult.Running;
        }

        public override void OnCompleted() {
            status = "completed";
        }

    }

    private class FailingTask : UITask {

        public string status;
        public int runCount;

        public override void OnInitialized() {
            status = "init";
        }

        public override void OnFailed() {
            status = "fail";
        }

        public override UITaskResult Run(float deltaTime) {
            runCount++;
            return UITaskResult.Failed;
        }

        public override void OnCompleted() {
            status = "complete";
        }

    }

    [Test]
    public void InitializeAndRunASingleTask() {
        UITaskSystem system = new UITaskSystem();
        Task0 task0 = new Task0(3);

        system.AddTask(task0);
        Assert.AreEqual(1, system.ActiveTaskCount);

        Assert.AreEqual(0, task0.variable);

        system.OnUpdate();
        Assert.AreEqual(1, task0.variable);
        Assert.AreEqual(1, task0.runCount);

        system.OnUpdate();

        Assert.AreEqual(1, task0.variable);
        Assert.AreEqual(2, task0.runCount);
        Assert.AreEqual(1, system.ActiveTaskCount);

        system.OnUpdate();

        Assert.AreEqual("completed", task0.status);
        Assert.AreEqual(3, task0.runCount);
        Assert.AreEqual(0, system.ActiveTaskCount);
    }

    [Test]
    public void InitializeAndFailASingleTask() {
        UITaskSystem system = new UITaskSystem();
        FailingTask task = new FailingTask();

        system.AddTask(task);
        Assert.AreEqual(null, task.status);

        system.OnUpdate();
        Assert.AreEqual("fail", task.status);
        Assert.AreEqual(1, task.runCount);

        Assert.AreEqual(0, system.ActiveTaskCount);
    }

    [Test]
    public void RunMultipleTasks() {
        UITaskSystem system = new UITaskSystem();
        Task0 task0 = new Task0(2);
        Task0 task1 = new Task0(3);
        Task0 task2 = new Task0(4);

        system.AddTask(task0);
        system.AddTask(task1);
        system.AddTask(task2);

        Assert.AreEqual(3, system.ActiveTaskCount);

        system.OnUpdate();

        Assert.AreEqual(UITaskState.Running, task0.state);
        Assert.AreEqual(UITaskState.Running, task1.state);
        Assert.AreEqual(UITaskState.Running, task2.state);

        system.OnUpdate();

        Assert.AreEqual(2, system.ActiveTaskCount);

        Assert.AreEqual(UITaskState.Completed, task0.state);
        Assert.AreEqual(UITaskState.Running, task1.state);
        Assert.AreEqual(UITaskState.Running, task2.state);

        system.OnUpdate();

        Assert.AreEqual(1, system.ActiveTaskCount);
        Assert.AreEqual(UITaskState.Completed, task0.state);
        Assert.AreEqual(UITaskState.Completed, task1.state);
        Assert.AreEqual(UITaskState.Running, task2.state);

        system.OnUpdate();

        Assert.AreEqual(0, system.ActiveTaskCount);

        Assert.AreEqual(UITaskState.Completed, task0.state);
        Assert.AreEqual(UITaskState.Completed, task1.state);
        Assert.AreEqual(UITaskState.Completed, task2.state);
    }

    [Test]
    public void CancelATask() {
        UITaskSystem system = new UITaskSystem();
        Task0 task0 = new Task0(2, UITaskResult.Cancelled);

        system.AddTask(task0);

        system.OnUpdate();
        Assert.AreEqual(1, system.ActiveTaskCount);
        Assert.AreEqual(UITaskState.Running, task0.state);
        system.OnUpdate();
        Assert.AreEqual(0, system.ActiveTaskCount);
        Assert.AreEqual(UITaskState.Cancelled, task0.state);
        Assert.AreEqual("cancelled", task0.status);
    }

    [Test]
    public void CancelATaskExternally() {
        UITaskSystem system = new UITaskSystem();
        Task0 task0 = new Task0(3, UITaskResult.Completed);

        system.AddTask(task0);

        system.OnUpdate();
        Assert.AreEqual(1, system.ActiveTaskCount);
        Assert.AreEqual(UITaskState.Running, task0.state);
        system.OnUpdate();
        Assert.AreEqual(1, system.ActiveTaskCount);

        task0.Cancel();

        Assert.AreEqual(UITaskState.Cancelled, task0.state);
        Assert.AreEqual("cancelled", task0.status);
        Assert.AreEqual(0, system.ActiveTaskCount);
    }

    [Test]
    public void FailATaskExternally() {
        UITaskSystem system = new UITaskSystem();
        Task0 task0 = new Task0(3, UITaskResult.Completed);

        system.AddTask(task0);

        system.OnUpdate();
        Assert.AreEqual(1, system.ActiveTaskCount);
        Assert.AreEqual(UITaskState.Running, task0.state);
        system.OnUpdate();
        Assert.AreEqual(1, system.ActiveTaskCount);

        task0.Fail();

        Assert.AreEqual(UITaskState.Failed, task0.state);
        Assert.AreEqual("failed", task0.status);
        Assert.AreEqual(0, system.ActiveTaskCount);
    }

    [Test]
    public void CompleteATaskExternally() {
        UITaskSystem system = new UITaskSystem();
        Task0 task0 = new Task0(3, UITaskResult.Completed);

        system.AddTask(task0);

        system.OnUpdate();
        Assert.AreEqual(1, system.ActiveTaskCount);
        Assert.AreEqual(UITaskState.Running, task0.state);
        system.OnUpdate();
        Assert.AreEqual(1, system.ActiveTaskCount);

        task0.Complete();

        Assert.AreEqual(UITaskState.Completed, task0.state);
        Assert.AreEqual("completed", task0.status);
        Assert.AreEqual(0, system.ActiveTaskCount);
    }

    [Test]
    public void RestartATaskAfterRunning() {
        UITaskSystem system = new UITaskSystem();

        Func<UITask, float, UITaskResult> task0 = (UITask t, float d) => {
            if (t.FrameCount == 2) {
                return UITaskResult.Restarted;
            }

            if (t.FrameCount == 4) {
                return UITaskResult.Completed;
            }

            return UITaskResult.Running;
        };

        UITask task = system.AddTask(task0);

        system.OnUpdate();
        Assert.AreEqual(1, system.ActiveTaskCount);
        Assert.AreEqual(UITaskState.Running, task.state);
        system.OnUpdate();
        Assert.AreEqual(1, system.ActiveTaskCount);
        Assert.AreEqual(UITaskState.Restarting, task.state);
        system.OnUpdate();
        system.OnUpdate();
        Assert.AreEqual(0, system.ActiveTaskCount);
        Assert.AreEqual(UITaskState.Completed, task.state);
    }

}