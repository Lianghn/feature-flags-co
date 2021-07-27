export interface ruleType {
    label: string;
    value: string;
    type?: string;
    default?: string;
}

export const ruleKeyConfig: ruleType[] = [
    {
        label: 'KeyId',
        value: 'KeyId'
    },{
        label: 'Name',
        value: 'Name'
    },{
        label: 'Email',
        value: 'Email'
    },{
        label: 'Customized Properties',
        value: 'Properties'
    }
]

export const ruleValueConfig: ruleType[] = [
    {
        label: 'Is True',
        value: 'IsTrue',
        type: '',
        default: 'IsTrue'
    },{
        label: 'Is False',
        value: 'IsFalse',
        type: '',
        default: 'IsFalse'
    },{
        label: '=',
        value: 'Equal',
        type: 'string'
    },{
        label: '!=',
        value: 'NotEqual',
        type: 'string',
        default: ''
    },{
        label: '<',
        value: 'LessThan',
        type: 'number',
        default: ''
    },{
        label: '>',
        value: 'BiggerThan',
        type: 'number',
        default: ''
    },{
        label: '<=',
        value: 'LessEqualThan',
        type: 'number',
        default: ''
    },{
        label: '>=',
        value: 'BiggerEqualThan',
        type: 'number',
        default: ''
    },{
        label: 'Is one of',
        value: 'IsOneOf',
        type: 'multi',
        default: ''
    },{
        label: 'Not one of',
        value: 'NotOneOf',
        type: 'multi',
        default: ''
    },{
        label: 'Contains',
        value: 'Contains',
        type: 'string',
        default: ''
    },{
        label: 'Not Contain',
        value: 'NotContain',
        type: 'string',
        default: ''
    },{
        label: 'Starts With',
        value: 'StartsWith',
        type: 'string',
        default: ''
    },{
        label: 'Ends With',
        value: 'EndsWith',
        type: 'string',
        default: ''
    }
]

export function findIndex(id: string) {
    return ruleValueConfig.findIndex((item: ruleType) => item.value === id);
}