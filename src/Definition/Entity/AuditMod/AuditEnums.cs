namespace Entity.AuditMod;

public enum AuditMode { Any, All }
public enum AuditRecordStatus { InProgress, Approved, Rejected, Withdrawn }
public enum AuditStepStatus { Waiting, Pending, Approved, Rejected, Cancelled }
public enum AuditTaskStatus { Waiting, Pending, Approved, Rejected, Cancelled }
public enum AuditDecision { Approve, Reject }

