import { Component, EventEmitter, Input, Output, } from '@angular/core';
import { IFftuwmtrParams, IJsonContent, IUserType } from '../../types/switch-new';
import { ruleType, ruleValueConfig } from './ruleConfig';

@Component({
  selector: 'find-rule',
  templateUrl: './find-rule.component.html',
  styleUrls: ['./find-rule.component.less']
})
export class FindRuleComponent {

  @Input()
  set data(value: IFftuwmtrParams) {
    this.ruleContentList = [];
    // 新创建的
    if(!value.ruleId) {
      this.ruleContentList.push({
        property: '',
        operation: '',
        value: '',
        multipleValue: []
      });
      this.variationRuleValue = null;
      this.percentageRolloutForFalse = null;
      this.percentageRolloutForTrue = null;
    } else {
      value.ruleJsonContent.forEach((item: IJsonContent) => {
        let result: ruleType = ruleValueConfig.filter((rule: ruleType) => rule.value === item.operation)[0];

        let defaultValue: string = '';
        let multipleValue: string[] = [];

        if(result.type === 'multi') {
          multipleValue = JSON.parse(item.value);
          defaultValue = '';
        } else {
          defaultValue = item.value;
          multipleValue = [];
        }
        this.ruleContentList.push({
          property: item.property,
          operation: item.operation,
          value: defaultValue,
          multipleValue: [...multipleValue],
          type: result.type
        })
      })

      this.variationRuleValue = value.variationRuleValue;
      this.percentageRolloutForFalse = value.percentageRolloutForFalse;
      this.percentageRolloutForTrue = value.percentageRolloutForTrue;
    }
  }

  @Input() properties: string[] = [];                 // 字段配置
  @Input() index: number = 0;

  @Output() deleteRule = new EventEmitter();          // 删除规则
  @Output() percentageChange = new EventEmitter<{ serve:boolean, T: number, F: number }>();     // serve 配置发生改变
  @Output() ruleConfigChange = new EventEmitter<IJsonContent[]>();

  public ruleContentList: IJsonContent[] = [];     // 规则列表
  public variationRuleValue: boolean | string = null;          // serve 的值 true false null
  public percentageRolloutForTrue: number = null;        // serve 为 null 时的 状态为 true 的百分比
  public percentageRolloutForFalse: number = null;       // serve 为 null 时的 状态为 false 的百分比

  // 添加规则
  onAddRule() {
    this.ruleContentList.push({
      property: '',
      operation: '',
      value: '',
      multipleValue: []
    })
  }

  // 删除规则
  onDeleteRule() {
    this.deleteRule.next();
  }

  // serve 配置值
  public onPercentageChange(value: { serve: boolean, T: number, F: number }) {
    this.percentageChange.next({
      serve: value.serve,
      T: value.T,
      F: value.F
    })
  }

  // 删除规则条目
  public onDeleteRuleItem(index: number) {
    if(this.ruleContentList.length === 1) {
      this.ruleContentList[0] = {
        property: '',
        operation: '',
        value: '',
        multipleValue: []
      }
    } else {
      this.ruleContentList.splice(index, 1);
    }
  }

  // 刷新数据
  public onRuleChange(value: IJsonContent, index: number) {
    this.ruleContentList[index] = value;
    this.ruleConfigChange.next(this.ruleContentList);
  }

  // 确认删除规则
  public confirm() {
    this.onDeleteRule();
  }
}
 