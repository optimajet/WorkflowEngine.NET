db.WorkflowProcessInstance.find({ 'Status.RuntimeId': { $exists: false } }).forEach(
    function (elem) {
        db.WorkflowProcessInstance.update(
            {
                _id: elem._id
            },
            {
                $set: {
                    'Status.RuntimeId': '00000000-0000-0000-0000-000000000000'
                }
            }
        );
    }
);

db.WorkflowRuntime.insert(
  { RuntimeId: "00000000-0000-0000-0000-000000000000", Lock: BinData(3,UUID().base64()), Status: NumberInt(100) }
);

db.WorkflowProcessInstance.find({ 'Status.SetTime': { $exists: false } }).forEach(
    function (elem) {
        db.WorkflowProcessInstance.update(
            {
                _id: elem._id
            },
            {
                $currentDate: {
                    "Status.SetTime": true
                 }
            }
        );
    }
);

db.WorkflowSync.insert(
    { Name: "Timer", Lock: BinData(3,UUID().base64()) }
);
db.WorkflowSync.insert(
  { Name: "ServiceTimer", Lock: BinData(3,UUID().base64()) }
);

db.WorkflowProcessTimer.find().forEach(function (timer) {
    
    var instance = db.WorkflowProcessInstance.findOne({ _id: timer.ProcessId });
    
    if (instance) {
        timer.RootProcessId = instance.RootProcessId;
        db.WorkflowProcessTimer.save(timer);
    }
});
