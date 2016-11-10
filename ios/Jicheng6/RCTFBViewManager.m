//
//  RCTFBViewManager.m
//  Jicheng6
//
//  Created by guojicheng on 16/11/10.
//  Copyright © 2016年 Facebook. All rights reserved.
//

#import "RCTFBViewManager.h"
#import "RCTUIManager.h"
#import "RCTFBView.h"

@implementation RCTFBViewManager

RCT_EXPORT_MODULE();
@synthesize bridge = _bridge;

-(UIView *)view
{
  return [[RCTFBView alloc]init];
}

- (dispatch_queue_t)methodQueue
{
  return dispatch_get_main_queue();
}

@end
