import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { FfcAngularSdkService } from 'ffc-angular-sdk';
import { NzMessageService } from 'ng-zorro-antd/message';
import { ExperimentService } from 'src/app/services/experiment.service';
import { SwitchService } from 'src/app/services/switch.service';
import { CSwitchParams, IFfParams, IPrequisiteFeatureFlag, IVariationOption } from '../types/switch-new';
import { differenceInCalendarDays, setHours } from 'date-fns';
import { map } from 'rxjs/operators';

@Component({
  selector: 'experimentation',
  templateUrl: './experimentation.component.html',
  styleUrls: ['./experimentation.component.less']
})
export class ExperimentationComponent implements OnInit {

  featureFlagId: string;
  currentVariationOptions: string[] = [];
  selectedBaseline: string = null;

  experimentation: string;

  hasInvalidVariation: boolean = false;
  hasWinnerVariation: boolean = false;
  constructor(
    private route: ActivatedRoute,
    private switchServe: SwitchService,
    private message: NzMessageService,
    private experimentService: ExperimentService,
    private ffcAngularSdkService: FfcAngularSdkService
  ) {
    this.experimentation = 'final version';//this.ffcAngularSdkService.variation('experimentation');

    this.route.data.pipe(map(res => res.switchInfo))
    .subscribe((result: CSwitchParams) => {
      const featureDetail = new CSwitchParams(result);
      this.currentVariationOptions = featureDetail.getVariationOptions().map(v => v.variationValue);
    })

    this.experimentResult = [...this.dataSet]
    .map((d, idx) => {
      return Object.assign({}, d, {
        id: idx,
        conversionRate: (d.conversionRate * 100).toFixed(1),
        confidenceInterval: d.confidenceInterval.map(c => c * 100),
        changeToBaseline: (d.changeToBaseline * 100).toFixed(1),
        isBaseline: true
      });
    });

    this.hasInvalidVariation = this.experimentResult.findIndex(e => e.isInvalid) > -1;
    this.hasWinnerVariation = this.experimentResult.findIndex(e => e.isWinner) > -1;
  }

  ngOnInit(): void {
    this.featureFlagId = this.route.snapshot.paramMap.get('id');
    if(this.switchServe.envId) {
      this.initData();
    }
  }

  disabledDate = (current: Date): boolean =>
    // Can not select days before today and today
    differenceInCalendarDays(current, new Date()) > 0;

  conversionTableTitle = '';
  isLoading: boolean = false;
  customEventList: string[] = [];
  selectedEvent: string;
  dateRange:Date[] = [];
  private initData() {
    this.isLoading = true;
    this.experimentService.getCustomEvents(this.switchServe.envId).subscribe((result) => {
      if(result) {
        this.customEventList = result;
        this.isLoading = false;
      }
    }, _ => {
      this.message.error("数据加载失败，请重试!");
      this.isLoading = false;
    })
  }

  onChange(result: Date[]): void {
    this.dateRange = [...result];
  }

  experimentResult = [];
  onSubmit() {

    if (this.selectedBaseline === undefined || this.selectedBaseline === null) {
      this.message.warning('请选择基准特性！');
      return false;
    }

    if (this.selectedEvent === undefined || this.selectedEvent === null || this.selectedEvent === '') {
      this.message.warning('请选择事件！');
      return false;
    }

    if (this.dateRange.length !== 2) {
      this.message.warning('请设置起止时间！');
      return false;
    }


    this.experimentResult = [...this.dataSet]
    .map((d, idx) => {
      return Object.assign({}, d, {
        id: idx,
        conversionRate: (d.conversionRate * 100).toFixed(1),
        confidenceInterval: d.confidenceInterval.map(c => c * 100),
        changeToBaseline: (d.changeToBaseline * 100).toFixed(1),
        isBaseline: true
      });
    });

    this.hasInvalidVariation = this.experimentResult.findIndex(e => e.isInvalid) > -1;
    this.hasWinnerVariation = this.experimentResult.findIndex(e => e.isWinner) > -1;
  }

  dataSet = [
    {
        "changeToBaseline": 0.19,
        "confidenceInterval": [
            0.51,
            0.90
        ],
        "conversion": 6,
        "conversionRate": 0.86,
        "isInvalid": false,
        "isWinner": true,
        "pValue": 0.46,
        "uniqueUsers": 7,
        "variation": "Green"
    },
    {
        "changeToBaseline": 0,
        "confidenceInterval": [
            0,
            1
        ],
        "conversion": 2,
        "conversionRate": 0.67,
        "isInvalid": false,
        "isWinner": false,
        "pValue": 0,
        "uniqueUsers": 3,
        "variation": "A"
    }
  ]
}
