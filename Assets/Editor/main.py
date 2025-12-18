import os
import sys
import json
import glob
import pandas as pd
import re

def parse_dialogueId_datas(dialogueId_str):
    """解析dialogueIdDatas字符串，转换为列表结构"""
    if pd.isna(dialogueId_str) or dialogueId_str == "":
        return []
    
    dialogueId_list = []
    
    # 分割多个dialogueId，使用分号分隔
    dialogueIds = str(dialogueId_str).split(';')
    
    for dialogueId in dialogueIds:
        dialogueId = dialogueId.strip()
        if not dialogueId:
            continue
            
        # 解析每个dialogueId的键值对
        dialogueId_dict = {}
        pairs = dialogueId.split(',')
        
        for pair in pairs:
            pair = pair.strip()
            if ':' in pair:
                key, value = pair.split(':', 1)
                key = key.strip()
                value = value.strip()
                
                # 根据键名映射到正确的JSON字段名]
                if key == 'id':
                    # 尝试转换为数值类型
                    try:
                        dialogueId_dict[key] = float(value) if '.' in value else int(value)
                    except ValueError:
                        dialogueId_dict[key] = value
        
        dialogueId_list.append(dialogueId_dict)
    
    return dialogueId_list

def parse_option_datas(option_str):
    """解析optionDatas字符串，转换为列表结构"""
    if pd.isna(option_str) or option_str == "":
        return []
    
    option_list = []
    
    # 分割多个option，使用分号分隔
    options = str(option_str).split(';')
    
    for option in options:
        option = option.strip()
        if not option:
            continue
            
        # 解析每个option的键值对
        option_dict = {}
        pairs = option.split(',')
        
        for pair in pairs:
            pair = pair.strip()
            if ':' in pair:
                key, value = pair.split(':', 1)
                key = key.strip()
                value = value.strip()
                
                # 根据键名映射到正确的JSON字段名
                if key == 'text':
                    option_dict['text'] = value
                elif key == 'targetId':
                    # 尝试转换为数值类型
                    try:
                        option_dict[key] = float(value) if '.' in value else int(value)
                    except ValueError:
                        option_dict[key] = value
                elif key == 'isTakeTask':
                    # 只有当字符串（忽略大小写）为 'true' 时才转为 True，否则为 False
                    option_dict['isTakeTask'] = (str(value).lower() == 'true')
                else:
                    option_dict[key] = value
        
        option_list.append(option_dict)
    
    return option_list

def parse_modifier_datas(modifier_str):
    """解析modifierDatas字符串，转换为列表结构"""
    if pd.isna(modifier_str) or modifier_str == "":
        return []
    
    modifier_list = []
    
    # 分割多个modifier，使用分号分隔
    modifiers = str(modifier_str).split(';')
    
    for modifier in modifiers:
        modifier = modifier.strip()
        if not modifier:
            continue
            
        # 解析每个modifier的键值对
        modifier_dict = {}
        pairs = modifier.split(',')
        
        for pair in pairs:
            pair = pair.strip()
            if ':' in pair:
                key, value = pair.split(':', 1)
                key = key.strip()
                value = value.strip()
                
                # 根据键名映射到正确的JSON字段名
                if key == 'modifier':
                    modifier_dict['modifierType'] = value
                elif key == 'value':
                    # 尝试转换为数值类型
                    try:
                        modifier_dict[key] = float(value) if '.' in value else int(value)
                    except ValueError:
                        modifier_dict[key] = value
                else:
                    modifier_dict[key] = value
        
        # 确保有必要的字段
        if 'modifierType' in modifier_dict:
            modifier_list.append(modifier_dict)
    
    return modifier_list

def parse_quest_datas(quest_str):
    """解析modifierDatas字符串，转换为列表结构"""
    if pd.isna(quest_str) or quest_str == "":
        return []
    
    quest_list = []
    
    quests = str(quest_str).split(';')
    
    for quest in quests:
        quest = quest.strip()
        if not quest:
            continue
            
        quest_dict = {}
        pairs = quest.split(',')
        
        for pair in pairs:
            pair = pair.strip()
            if ':' in pair:
                key, value = pair.split(':', 1)
                key = key.strip()
                value = value.strip()
                
                # 根据键名映射到正确的JSON字段名
                if key == 'name':
                    quest_dict['name'] = value
                elif key == 'requireAmount':
                    # 尝试转换为数值类型
                    try:
                        quest_dict[key] = float(value) if '.' in value else int(value)
                    except ValueError:
                        quest_dict[key] = value
                elif key == 'requireType':
                    # 尝试转换为数值类型
                    try:
                        quest_dict[key] = float(value) if '.' in value else int(value)
                    except ValueError:
                        quest_dict[key] = value
                else:
                    quest_dict[key] = value
        
        # 确保有必要的字段
        # if 'modifierType' in quest_dict:
        quest_list.append(quest_dict)
    
    return quest_list

def parse_item_datas(item_str):
    """解析modifierDatas字符串，转换为列表结构"""
    if pd.isna(item_str) or item_str == "":
        return []
    
    item_list = []
    
    items = str(item_str).split(';')
    
    for item in items:
        item = item.strip()
        if not item:
            continue
            
        item_dict = {}
        pairs = item.split(',')
        
        for pair in pairs:
            pair = pair.strip()
            if ':' in pair:
                key, value = pair.split(':', 1)
                key = key.strip()
                value = value.strip()
                
                # 根据键名映射到正确的JSON字段名
                if key == 'itemInfoType':
                    # 尝试转换为数值类型
                    try:
                        item_dict[key] = float(value) if '.' in value else int(value)
                    except ValueError:
                        item_dict[key] = value
                elif key == 'quantity':
                    # 尝试转换为数值类型
                    try:
                        item_dict[key] = float(value) if '.' in value else int(value)
                    except ValueError:
                        item_dict[key] = value
                elif key == 'itemId':
                    # 尝试转换为数值类型
                    try:
                        item_dict[key] = float(value) if '.' in value else int(value)
                    except ValueError:
                        item_dict[key] = value
                elif key == 'index':
                    # 尝试转换为数值类型
                    try:
                        item_dict[key] = float(value) if '.' in value else int(value)
                    except ValueError:
                        item_dict[key] = value
                else:
                    item_dict[key] = value
        
        # 确保有必要的字段
        # if 'modifierType' in item_dict:
        item_list.append(item_dict)
    
    return item_list

def excel_to_json(excel_file_path, json_file_path):
    # 读取Excel文件
    df = pd.read_excel(excel_file_path, engine='openpyxl')
    
    # 处理数据转换 - 确保正确的数值类型
    records = []
    for _, row in df.iterrows():
        record = {}
        
        for col in df.columns:
            # print(col)

            value = row[col]

            # 处理modifierDatas[list]列
            if 'modifierInfos' in col:
                record['modifierInfos'] = parse_modifier_datas(value)
            elif 'questRequireInfos' in col:
                record['questRequireInfos'] = parse_quest_datas(value)
            elif 'rewardItemInfos' in col:
                record['rewardItemInfos'] = parse_item_datas(value)
            elif 'dialogueOptionList' in col:
                record['dialogueOptionList'] = parse_option_datas(value)
            elif 'questDialogueBindingIdList' in col:
                record['questDialogueBindingIdList'] = parse_dialogueId_datas(value)
            else:
                if pd.isna(value):
                    record[col] = None
                # 先判断是否为bool类型，Excel中的bool类型也会被解析为int、float类型
                elif isinstance(value, bool):
                    # print(value, record[col])
                    record[col] = bool(value)
                elif isinstance(value, (int, float)):
                    # 保持原有的数值类型
                    if isinstance(value, int) or value.is_integer():
                        record[col] = int(value)  # 整数
                    else:
                        record[col] = float(value)  # 浮点数

                else:
                    # 尝试将字符串转换为数值
                    str_value = str(value).strip()
                    try:
                        if '.' in str_value:
                            record[col] = float(str_value)
                        else:
                            record[col] = int(str_value)
                    except (ValueError, TypeError):
                        record[col] = str_value
        
        records.append(record)
    
    # 保存为JSON文件
    with open(json_file_path, 'w', encoding='utf-8') as f:
        json.dump(records, f, ensure_ascii=False, indent=4)
    
    # 保存为JSON文件
    with open(json_file_path, 'w', encoding='utf-8') as f:
        json.dump(records, f, ensure_ascii=False, indent=4)

def json_to_excel(json_file_path, excel_file_path):
    # 1. 读取JSON文件
    with open(json_file_path, 'r', encoding='utf-8') as f:
        data = json.load(f)

    # 处理modifierDatas列表，转换为字符串格式
    for item in data:
        if 'modifierDatas' in item and isinstance(item['modifierDatas'], list):
            modifier_strs = []
            for modifier in item['modifierDatas']:
                modifier_parts = []
                # 将modifierType映射回modifier
                if 'modifierType' in modifier:
                    modifier_parts.append(f"modifier:{modifier['modifierType']}")
                # 添加value
                if 'value' in modifier:
                    modifier_parts.append(f"value:{modifier['value']}")
                # 添加其他字段
                for key, val in modifier.items():
                    if key not in ['modifierType', 'value']:
                        modifier_parts.append(f"{key}:{val}")
                
                modifier_strs.append(','.join(modifier_parts))
            
            item['modifierDatas'] = ';'.join(modifier_strs)
        elif 'modifierDatas' in item:
            # 如果modifierDatas不是列表，保持原样
            pass

    # 2. 转换为DataFrame
    df = pd.DataFrame(data)

    # 3. 保存为Excel文件
    df.to_excel(excel_file_path, index=False, engine='openpyxl')

# 传入文件后缀
def get_suffix_filename(file_path, suffix):
    # 获取所有指定后缀的文件（包括完整路径）
    files = glob.glob(f"{file_path}/*.{suffix.replace('.', '')}")

    # 仅提取文件名（不带路径）
    file_names = [os.path.basename(file).split(".")[0] for file in files]

    return file_names, files

# 使用示例
if __name__ == "__main__":
    # 从命令行参数获取路径和转换方向
    isJsonToExcel = sys.argv[1] == "1"
    json_file_path = sys.argv[2]  # JSON文件路径
    excel_file_path = sys.argv[3]  # Excel文件路径

    if isJsonToExcel:
        file_path = json_file_path
        suffix = "json"
    else:
        file_path = excel_file_path
        suffix = "xlsx"

    file_name_list, full_path_list = get_suffix_filename(file_path, suffix)
    
    for i, file_name in enumerate(file_name_list):
        if isJsonToExcel:
            json_full_path = full_path_list[i]
            excel_full_path = os.path.join(excel_file_path, file_name + ".xlsx")
            json_to_excel(json_full_path, excel_full_path)
        else:
            excel_full_path = full_path_list[i]
            json_full_path = os.path.join(json_file_path, file_name + ".json")
            excel_to_json(excel_full_path, json_full_path)

    print(f"switch ok, switch file num = {len(file_name_list)}")