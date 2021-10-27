db.WorkflowProcessInstance.find().forEach(function (instance) {
    instance.CreationDate = new Date();
    db.WorkflowProcessInstance.save(instance);
});

db.WorkflowInbox.find().forEach(function (instance) {
	instance.AddingDate = new Date();
    db.WorkflowProcessInstance.save(instance);
});
