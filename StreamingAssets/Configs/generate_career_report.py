import sys
import os
import json
from datetime import datetime
from reportlab.lib.pagesizes import letter
from reportlab.lib import colors
from reportlab.lib.units import inch
from reportlab.lib.styles import getSampleStyleSheet, ParagraphStyle
from reportlab.lib.enums import TA_CENTER, TA_LEFT, TA_RIGHT
from reportlab.platypus import (
    SimpleDocTemplate, Paragraph, Spacer, Table, TableStyle,
    PageBreak, Image, Flowable, KeepTogether
)

BRAND_BLUE = colors.HexColor("#002D72")
BRAND_GOLD = colors.HexColor("#C5A246")
ACCENT_LIGHT = colors.HexColor("#f5f5f5")

script_dir = os.path.dirname(os.path.abspath(__file__))
logo_filename = os.path.join(script_dir, "cropped-1024px-UWC_logo.svg_-1.png")
star_filled_path = os.path.join(script_dir, "star_filled.png")
star_empty_path = os.path.join(script_dir, "star_empty.png")

class StarRating(Flowable):
    def __init__(self, score, star_size=11, spacing=1):
        super().__init__()
        self.score = score
        self.star_size = star_size
        self.spacing = spacing
        self.width = star_size * 5 + spacing * 4  
        self.height = star_size
        
    def draw(self):
        c = self.canv
        c.saveState()
        
        if self.score >= 80:
            filled = 5
        elif self.score >= 70:
            filled = 4
        elif self.score >= 60:
            filled = 3
        elif self.score >= 50:
            filled = 2
        else:
            filled = 1
        
        x_pos = 0
        for i in range(5):
            try:
                if i < filled:
                    c.drawImage(star_filled_path, x_pos, 0, 
                              width=self.star_size, height=self.star_size,
                              preserveAspectRatio=True, mask='auto')
                else:
                    c.drawImage(star_empty_path, x_pos, 0,
                              width=self.star_size, height=self.star_size, 
                              preserveAspectRatio=True, mask='auto')
            except:
                pass
            x_pos += self.star_size + self.spacing
        
        c.restoreState()

def get_strength_label(score):
    if score >= 80: return "Excellent"
    elif score >= 70: return "Strong"
    elif score >= 60: return "Good"
    elif score >= 50: return "Moderate"
    elif score >= 40: return "Developing"
    else: return "Needs Development"

def get_star_rating(aai_score):
    return StarRating(aai_score, star_size=11, spacing=1)

def interpret_riasec_code(code):
    interpretations = {
        'R': 'Realistic (hands-on, practical, mechanical)',
        'I': 'Investigative (analytical, scientific, curious)',
        'A': 'Artistic (creative, expressive, imaginative)',
        'S': 'Social (helpful, empathetic, collaborative)',
        'E': 'Enterprising (persuasive, ambitious, leadership-oriented)',
        'C': 'Conventional (organized, detail-oriented, structured)'
    }
    if not code: return "Unable to determine interests"
    primary = code[0]
    secondary = code[1] if len(code)>1 else None
    tertiary = code[2] if len(code)>2 else None
    parts = [f"You are primarily {interpretations.get(primary,'Unknown')}."]
    if secondary: parts.append(f"Secondary: {interpretations.get(secondary,'Unknown')}.")
    if tertiary: parts.append(f"Tertiary: {interpretations.get(tertiary,'Unknown')}.")
    return " ".join(parts)

def interpret_big_five(trait_name, score):
    interpretations = {
        'O': {'high':'Curious, imaginative, open to new experiences and ideas','moderate':'Balanced between tradition and novelty','low':'Practical, conventional, prefers familiar approaches'},
        'C': {'high':'Disciplined, organized, reliable, goal-oriented','moderate':'Adequately organized with some flexibility','low':'Spontaneous, flexible, less concerned with structure'},
        'E': {'high':'Outgoing, energetic, seeks social interaction','moderate':'Comfortable in both social and solitary settings','low':'Reserved, introspective, prefers solitary activities'},
        'A': {'high':'Cooperative, compassionate, values harmony','moderate':'Balanced between cooperation and assertiveness','low':'Competitive, skeptical, values independence'},
        'N': {'high':'Emotionally sensitive, experiences stress more intensely','moderate':'Generally stable with occasional emotional fluctuations','low':'Emotionally stable, resilient, calm under pressure'}
    }
    if score >= 65: level='high'
    elif score >=45: level='moderate'
    else: level='low'
    return interpretations.get(trait_name,{}).get(level,'No interpretation available')

class BlockBar(Flowable):
    def __init__(self, score, width=100, height=12, blocks=20):  
        super().__init__()
        self.score = score
        self.width = width
        self.height = height
        self.blocks = blocks

    def draw(self):
        c = self.canv
        block_width = self.width / self.blocks
        filled_blocks = int(round(self.blocks * self.score / 100))
        for i in range(self.blocks):
            x = i*block_width
            fill_color = BRAND_GOLD if i<filled_blocks else BRAND_BLUE
            c.setFillColor(fill_color)
            c.setStrokeColor(BRAND_BLUE)
            c.rect(x,0,block_width,self.height,stroke=1,fill=1)
        c.setFillColor(BRAND_BLUE)
        c.setFont("Helvetica",8)
        c.drawString(self.width+4, self.height/4, f"{self.score:.0f}%")

class MainHeading(Flowable):
    def __init__(self, text, width, height=40, font_size=18, color=BRAND_BLUE):
        super().__init__()
        self.text = text
        self.width = width
        self.height = height
        self.font_size = font_size
        self.color = color
    
    def draw(self):
        c = self.canv
        c.saveState()
        c.setStrokeColor(self.color)
        c.setLineWidth(2) 
        
        c.line(0, self.height - 2, self.width, self.height - 2)
        c.line(0, self.height - 5, self.width, self.height - 5)
        
        c.setFillColor(self.color)
        c.setFont("Helvetica-Bold", self.font_size)
        c.drawCentredString(self.width/2, self.height/2 - self.font_size/4, self.text)
        
        c.line(0, 2, self.width, 2)
        c.line(0, 5, self.width, 5)
        
        c.restoreState()

class SectionHeading(Flowable):
    def __init__(self, text, width, height=30, font_size=12, color=BRAND_BLUE):
        super().__init__()
        self.text = text
        self.width = width
        self.height = height
        self.font_size = font_size
        self.color = color
    
    def draw(self):
        c = self.canv
        c.saveState()
        c.setStrokeColor(self.color)
        c.setLineWidth(1)
        
        c.line(0, self.height - 2, self.width, self.height - 2)
        
        c.setFillColor(self.color)
        c.setFont("Helvetica-Bold", self.font_size)
        c.drawString(0, self.height/2 - self.font_size/4, self.text)
        
        c.line(0, 2, self.width, 2)
        
        c.restoreState()

def draw_header_footer(canvas, doc):
    canvas.saveState()
    try:
        logo_img = Image(logo_filename, width=50, height=50)
        logo_img.drawOn(canvas, doc.pagesize[0]-60, doc.pagesize[1]-60)
    except: pass
    canvas.restoreState()

bullet_style = ParagraphStyle(
    'Bullet', leftIndent=18, bulletIndent=9, spaceAfter=2, fontSize=10, textColor=colors.HexColor('#333333')
)

def generate_report(data, output_path):
    doc=SimpleDocTemplate(output_path,pagesize=letter,rightMargin=0.6*inch,leftMargin=0.6*inch,topMargin=0.9*inch,bottomMargin=0.6*inch)
    story=[]
    styles=getSampleStyleSheet()
    body_style=ParagraphStyle('Body',parent=styles['Normal'],fontSize=10,leading=14,textColor=colors.HexColor('#333333'))
    
    story.append(Spacer(1, 10))  
    story.append(MainHeading("CAREERQUESTXR ASSESSMENT REPORT", width=doc.width))
    story.append(Spacer(1,20))  

    domains=data.get('domains',{})
    i_score=domains.get('I',0)
    c_score=domains.get('C',0)
    p_score=domains.get('P',0)
    b_score=domains.get('B',0)
    overall_avg=(i_score+c_score+p_score+b_score)/4.0

    duration_str = data.get('duration', 'N/A')
    if duration_str == 'N/A' or duration_str is None:
        duration_display = "N/A"
    else:
        try:
            duration_mins = int(duration_str)
            duration_display = f"{duration_mins}"
        except:
            duration_display = str(duration_str)

    centered_body_style = ParagraphStyle(
        'CenteredBody',
        parent=body_style,
        alignment=TA_CENTER
    )

    info_data=[
        [Paragraph(f"<b>Participant:</b> {data.get('username','N/A')}",centered_body_style),
         Paragraph(f"<b>Date:</b> {data.get('date','')}",centered_body_style)],
        [Paragraph(f"<b>Assessment Duration:</b> {duration_display} minutes",centered_body_style),
         Paragraph(f"<b>Overall Average:</b> {overall_avg:.1f}%",centered_body_style)]
    ]
    info_tbl=Table(info_data,colWidths=[3.5*inch,3.5*inch])
    info_tbl.setStyle(TableStyle([
        ('VALIGN',(0,0),(-1,-1),'TOP'),
        ('BOTTOMPADDING',(0,0),(-1,-1),6),
        ('ALIGN', (0,0), (-1,-1), 'CENTER')  
    ]))
    
    centered_info = Table([[info_tbl]], colWidths=[doc.width])
    centered_info.setStyle(TableStyle([
        ('ALIGN', (0,0), (-1,-1), 'CENTER'),
        ('VALIGN', (0,0), (-1,-1), 'MIDDLE')
    ]))
    
    story.append(centered_info)
    story.append(Spacer(1,12))

    story.append(SectionHeading("DOMAIN PERFORMANCE SUMMARY", width=doc.width))
    story.append(Spacer(1,8))
    
    domain_rows=[[Paragraph("<b>Domain</b>",body_style),
                  Paragraph("<b>Performance</b>",body_style),
                  Paragraph("<b>Level</b>",body_style)]]
    for code,name,score in [('I','Intellectual',i_score),('C','Cognitive',c_score),('P','Psychological',p_score),('B','Behavioral',b_score)]:
        level=get_strength_label(score)
        bar=BlockBar(score)
        domain_rows.append([Paragraph(name,body_style),bar,Paragraph(level,body_style)])
    
    domain_table=Table(domain_rows,colWidths=[1.5*inch,1.8*inch,1.8*inch])
    domain_table.setStyle(TableStyle([
        ('GRID',(0,0),(-1,-1),0.25,colors.HexColor('#e0e0e0')),
        ('BACKGROUND',(0,0),(-1,0),BRAND_GOLD),
        ('TEXTCOLOR',(0,0),(-1,0),colors.white),
        ('VALIGN',(0,0),(-1,-1),'MIDDLE'),
        ('LEFTPADDING',(0,0),(-1,-1),6),
        ('RIGHTPADDING',(0,0),(-1,-1),6),
        ('TOPPADDING',(0,0),(-1,-1),6),
        ('BOTTOMPADDING',(0,0),(-1,-1),6),
        ('ROWBACKGROUNDS',(0,1),(-1,-1),[colors.white,ACCENT_LIGHT])
    ]))
    story.append(domain_table)
    story.append(Spacer(1,12))

    story.append(SectionHeading("PERSONALITY PROFILE", width=doc.width))
    story.append(Spacer(1,8))
    
    riasec_code=data.get('dominant_riasec','')
    story.append(Paragraph(f"<b>RIASEC Code:</b> {riasec_code}",body_style))
    story.append(Spacer(1,6))
    
    riasec=data.get('riasec',{})
    riasec_names={'R':'Realistic','I':'Investigative','A':'Artistic','S':'Social','E':'Enterprising','C':'Conventional'}
    for code,name in riasec_names.items():
        score=riasec.get(code,0)
        story.append(Paragraph(f"• <b>{name}:</b> {score:.0f}%",bullet_style))
    story.append(Spacer(1,6))
    
    story.append(Paragraph(interpret_riasec_code(riasec_code),body_style))
    story.append(Spacer(1,8))

    story.append(Paragraph("<b>Big Five Personality Traits:</b>",body_style))
    story.append(Spacer(1,6))
    bigfive=data.get('bigfive',{})
    bigfive_names={'O':'Openness','C':'Conscientiousness','E':'Extraversion','A':'Agreeableness','N':'Neuroticism'}
    for code,name in bigfive_names.items():
        score=bigfive.get(code,0)
        interp=interpret_big_five(code,score)
        story.append(Paragraph(f"• <b>{name}:</b> {score:.0f}% — {interp}",bullet_style))
    story.append(Spacer(1,12))

    story.append(SectionHeading("AREAS OF STRENGTH & DEVELOPMENT", width=doc.width))
    story.append(Spacer(1,8))
    
    domain_list=[('Intellectual',i_score),('Cognitive',c_score),('Psychological',p_score),('Behavioral',b_score)]
    domain_list_sorted=sorted(domain_list,key=lambda x:x[1],reverse=True)
    strengths=[f"• {get_strength_label(score)} {name} ({score:.1f}%)" for name,score in domain_list_sorted if score>=60]
    
    if strengths:
        story.append(Paragraph("<b>Domain Strengths:</b>", body_style))
        story.append(Spacer(1,4))
        for s in strengths: 
            story.append(Paragraph(s,bullet_style))
            story.append(Spacer(1,2))
    else: 
        story.append(Paragraph("No domain scores met the threshold for strengths (≥ 60%).",bullet_style))
    
    story.append(Spacer(1,8))

    bf_strengths=[]
    for code,score in bigfive.items():
        if code=='N': continue
        if score>=60: bf_strengths.append(f"• {bigfive_names.get(code,code)} ({score:.0f}%)")
    neuro_score=bigfive.get('N',None)
    if neuro_score is not None and neuro_score<=40: bf_strengths.append(f"• Low Neuroticism (Emotionally stable) ({neuro_score:.0f}%)")
    
    if bf_strengths:
        story.append(Paragraph("<b>Personality Strengths:</b>",body_style))
        story.append(Spacer(1,4))
        for s in bf_strengths: 
            story.append(Paragraph(s,bullet_style))
            story.append(Spacer(1,2))
    else: 
        story.append(Paragraph("No Big Five traits reached the strength threshold (≥ 60%).",bullet_style))
    
    story.append(Spacer(1,10))
    
    story.append(Paragraph("<b>Development Opportunities:</b>", body_style))
    story.append(Spacer(1,4))
    
    weak_domains = [f"• {name} ({score:.1f}%)" for name,score in domain_list_sorted if score<60]
    
    if weak_domains or (not bf_strengths and neuro_score and neuro_score > 60):
        lowest_domain = domain_list_sorted[-1]
        dev_suggestion = ""
        if lowest_domain[0] == "Intellectual":
            dev_suggestion = "Consider engaging in analytical reading and problem-solving exercises"
        elif lowest_domain[0] == "Cognitive":
            dev_suggestion = "Practice memory training exercises and attention-building activities"
        elif lowest_domain[0] == "Psychological":
            dev_suggestion = "Develop stress management techniques and emotional awareness practices"
        elif lowest_domain[0] == "Behavioral":
            dev_suggestion = "Work on building persistence and self-regulation through structured goal-setting"
        
        story.append(Paragraph(
            f"• {lowest_domain[0]} domain ({lowest_domain[1]:.1f}%): {dev_suggestion}",
            bullet_style
        ))
        story.append(Spacer(1,2))
        
        if not bf_strengths:
            bf_list = [(code, score) for code, score in bigfive.items() if code != 'N']
            bf_list_sorted = sorted(bf_list, key=lambda x: x[1])
            if bf_list_sorted:
                lowest_bf = bf_list_sorted[0]
                if lowest_bf[1] < 60:
                    trait_name = bigfive_names.get(lowest_bf[0], lowest_bf[0])
                    story.append(Paragraph(
                        f"• Consider developing {trait_name} skills ({lowest_bf[1]:.0f}%)",
                        bullet_style
                    ))
                    story.append(Spacer(1,2))
        
        if neuro_score and neuro_score > 60:
            story.append(Paragraph(
                f"• Neuroticism ({neuro_score:.0f}%): Practice stress management and emotional regulation techniques",
                bullet_style
            ))
            story.append(Spacer(1,2))
    else:
        story.append(Paragraph("Continue maintaining your strong performance across all areas.",bullet_style))
    
    story.append(Spacer(1,12))

    story.append(PageBreak())
    story.append(SectionHeading("TOP 5 CAREER RECOMMENDATIONS", width=doc.width))
    story.append(Spacer(1,12))
    
    recommendations = data.get('recommendations', [])
    if not recommendations or len(recommendations) == 0:
        story.append(Paragraph(
            "No career recommendations could be generated based on your assessment results. "
            "Please consult with a career counselor for personalized guidance.",
            body_style
        ))
    else:
        for i, rec in enumerate(recommendations[:5], 1):
            career_name = rec.get('career_name', 'Unknown Career')
            
            if 'faculty' in rec and rec['faculty']:
                full_career_text = f"{career_name} (Faculty: {rec['faculty']})"
            else:
                full_career_text = career_name
            
            stars = get_star_rating(rec.get('fit_score', 0))
            
            header_table = Table([[Paragraph(f"<b>{i}.</b>", body_style), stars, 
                                   Paragraph(f"<b>{full_career_text}</b>", body_style)]], 
                                 colWidths=[0.25*inch, 0.9*inch, 5.35*inch])
            header_table.setStyle(TableStyle([
                ('VALIGN', (0,0), (-1,-1), 'MIDDLE'),
                ('LEFTPADDING', (0,0), (-1,-1), 0),
                ('RIGHTPADDING', (0,0), (-1,-1), 8),
                ('TOPPADDING', (0,0), (-1,-1), 2),
                ('BOTTOMPADDING', (0,0), (-1,-1), 2),
            ]))
            story.append(header_table)
            story.append(Spacer(1,6))
            
            fit_score = rec.get('fit_score', 0)
            fit_label = get_strength_label(fit_score)
            story.append(Paragraph(
                f"<b>Academic Alignment Index:</b> {fit_score:.1f} ({fit_label} Fit)",
                body_style
            ))
            story.append(Spacer(1,6))
            
            if 'explanation' in rec and rec['explanation']:
                story.append(Paragraph(f"<b>Why This Match:</b>", body_style))
                story.append(Spacer(1,4))
                story.append(Paragraph(rec['explanation'], body_style))
                story.append(Spacer(1,6))
            
            if 'career_paths' in rec and rec['career_paths']:
                story.append(Paragraph(
                    f"<b>Career Paths:</b> {rec['career_paths']}",
                    body_style
                ))
            
            story.append(Spacer(1,15))
    
    story.append(SectionHeading("NEXT STEPS", width=doc.width))
    story.append(Spacer(1,8))
    
    next_steps=[
        "Schedule appointment with UWC Career Services to discuss recommendations",
        "Research recommended programs: course structures, career outcomes, admission requirements",
        "Consider job shadowing or informational interviews in top 2-3 fields",
        "Prepare for National Benchmark Tests (NBTs) targeting requirements for your chosen fields",
        "Explore funding options: NSFAS, corporate bursaries, scholarships"
    ]
    for s in next_steps:
        story.append(Paragraph(f"• {s}",bullet_style))
        story.append(Spacer(1,4))

    story.append(Spacer(1,10))
    story.append(SectionHeading("IMPORTANT DISCLAIMER", width=doc.width))
    story.append(Spacer(1,8))
    
    disclaimer=("This report provides preliminary career guidance based on your CareerQuestXR assessment. "
                "It should be used as ONE input in your decision-making process, alongside:")
    story.append(Paragraph(disclaimer,body_style))
    story.append(Spacer(1,6))
    
    disclaimer_points = [
        "Your personal interests and values",
        "Feedback from teachers, counselors, and family",
        "Academic performance in relevant subjects",
        "Financial considerations and funding availability"
    ]
    for point in disclaimer_points:
        story.append(Paragraph(f"• {point}",bullet_style))
        story.append(Spacer(1,2))
    
    story.append(Spacer(1,8))
    disclaimer2=("CareerQuestXR is a proof-of-concept research system requiring further validation. "
                "Recommendations are not guarantees of success. All career decisions remain your responsibility.")
    story.append(Paragraph(disclaimer2,body_style))
    story.append(Spacer(1,8))
    
    disclaimer3="For questions or concerns, contact your school's career guidance counselor or UWC Career Services."
    story.append(Paragraph(disclaimer3,body_style))
    story.append(Spacer(1,18))
    
    story.append(MainHeading("END OF REPORT", width=doc.width, height=30, font_size=16))

    doc.build(story, onFirstPage=draw_header_footer, onLaterPages=draw_header_footer)
    print(f"PDF report generated successfully: {output_path}")

def main():
    if len(sys.argv)<3:
        print("Usage: python generate_career_report.py <input_json> <output_pdf>")
        sys.exit(1)
    input_file=sys.argv[1]
    output_file=sys.argv[2]
    try:
        with open(input_file,'r') as f:
            data=json.load(f)
        data.setdefault('domains',{'I':0,'C':0,'P':0,'B':0})
        data.setdefault('bigfive',{'O':0,'C':0,'E':0,'A':0,'N':0})
        data.setdefault('recommendations',[])
        data.setdefault('dominant_riasec','')
        generate_report(data,output_file)
        print(f"Report generated: {output_file}")
    except Exception as e:
        import traceback
        traceback.print_exc()
        print(f"Error generating report: {e}")
        sys.exit(1)

if __name__=="__main__":
    main()