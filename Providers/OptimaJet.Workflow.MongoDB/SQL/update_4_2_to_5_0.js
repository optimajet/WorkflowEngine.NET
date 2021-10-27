db.WorkflowProcessInstance.find().forEach(function (instance) {
    
    var scheme = db.WorkflowProcessScheme.findOne({ _id: instance.SchemeId });
    
    if (scheme) {
        instance.SubprocessName = scheme.StartingTransition;
        db.WorkflowProcessInstance.save(instance);
    }
});
