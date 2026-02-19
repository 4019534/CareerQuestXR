using System;
using System.Collections.Generic;

[Serializable]
public class RIASECCareerDB
{
    public List<RIASECEntry> RIASECFields;
}

[Serializable]
public class RIASECEntry
{
    public string code;
    public List<RoleDefinition> roles;
}

[Serializable]
public class RoleDefinition
{
    public string name;               
    public string field;                
    public DomainWeights domainWeights; 
    public RIASECWeights riasec;        
    public OCEANProfile oceanProfile;   
    public string description;         
    public string explanationTemplate;  
}


[Serializable]
public class DomainWeights
{
    public float I;
    public float C;
    public float P;
    public float B;
}

[Serializable]
public class RIASECWeights
{
    public float R; 
    public float I;
    public float A;
    public float S;
    public float E; 
    public float C; 
}

[Serializable]
public class OCEANProfile
{
    public float O; 
    public float C;
    public float E; 
    public float A; 
    public float N; 
}
