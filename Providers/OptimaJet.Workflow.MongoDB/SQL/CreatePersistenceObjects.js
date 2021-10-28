db.WorkflowSync.insert(
    { Name: "Timer", Lock: BinData(3,UUID().base64()) }
);
db.WorkflowSync.insert(
  { Name: "ServiceTimer", Lock: BinData(3,UUID().base64()) }
);

db.WorkflowRuntime.insert(
  { RuntimeId: "00000000-0000-0000-0000-000000000000", Lock: BinData(3,UUID().base64()), Status: NumberInt(100) }
);