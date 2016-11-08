//
//  KeyboardHeight.h
//  Jicheng6
//
//  Created by guojicheng on 16/11/7.
//  Copyright © 2016年 Facebook. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "RCTEventEmitter.h"
#import "RCTBridgeModule.h"

@interface KeyboardHeight : RCTEventEmitter<RCTBridgeModule>

-(void)heightChanged:(int)height;

@property (nonatomic, assign)int kbHeight;

@end
