#light

open System
open System.IO
open System.Drawing
open System.Drawing.Imaging
open System.Threading
open System.Windows.Forms

let earth_mass = 6.0E24
let earth_radius = 6.357E6
let G = 6.67428E-11
let mu = G * earth_mass

let tbr = 42164000.0

let scale = 400

let debug = true

let font = new Font("Arial",float32 12)
let solidBrush = new SolidBrush(Color.Black)

let mutable time:uint64 = uint64 0

type VM(p0 : byte[],image : Bitmap (**), form : Form) = 
    let mutable status = false
    let mutable done_first_thrust = false
    let mutable done_second_thrust = false
    
    let instructions = 
        [| for i in 0..12..(p0.Length-12) -> 
            let index = i / 12
            if (index % 2 = 0)
            then ((uint32 p0.[i+8])) + ((uint32 p0.[i+9])<<<8) + ((uint32 p0.[i+10])<<<16) + ((uint32 p0.[i+11])<<<24) ; 
            else ((uint32 p0.[i]))   + ((uint32 p0.[i+1])<<<8) + ((uint32 p0.[i+2])<<<16)  + ((uint32 p0.[i+3])<<<24) ; 
        |] 
    
    let memory: float array = 
        [| for i in 0..12..(p0.Length-12) -> 
            let index = i / 12
            if (index % 2 = 0)
            then BitConverter.Int64BitsToDouble((int64 p0.[i])   + ((int64 p0.[i+1])<<<8) + ((int64 p0.[i+2])<<<16) + ((int64 p0.[i+3])<<<24) + ((int64 p0.[i+4])<<<32) + ((int64 p0.[i+5])<<<40) + ((int64 p0.[i+6])<<<48) + ((int64 p0.[i+7])<<<56)) ; 
            else BitConverter.Int64BitsToDouble((int64 p0.[i+4]) + ((int64 p0.[i+5])<<<8) + ((int64 p0.[i+6])<<<16) + ((int64 p0.[i+7])<<<24) + ((int64 p0.[i+8])<<<32) + ((int64 p0.[i+9])<<<40) + ((int64 p0.[i+10])<<<48) + ((int64 p0.[i+11])<<<56)) ;
        |]   
   
    
    let outport = Array.create (int Int16.MaxValue) (0.0)
    let inport = Array.create (int Int16.MaxValue) (0.0)
    
    let interpretSType eip op (r1:uint32) (r2:uint32) =
        match op with
        |1u -> memory.[eip] <- memory.[int r1] + memory.[int r2] ; //printf "Add %f %f\n" memory.[int r1]  memory.[int r2]
        |2u -> memory.[eip] <- memory.[int r1] - memory.[int r2] ; //printf "Sub %f %f\n" memory.[int r1]  memory.[int r2]
        |3u -> memory.[eip] <- memory.[int r1] * memory.[int r2] ; //printf "Mult mem[%d] mem[%d]\n" r1 r2
        |4u -> (memory.[eip] <- if memory.[int r2] = 0.0  then 0.0 else memory.[int r1] / memory.[int r2]) ; //printf "Div %f %f\n" memory.[int r1]  memory.[int r2]
        |5u -> outport.[int r1] <- memory.[int r2]  ; //printf "Output %d %f\n" r1  memory.[int r2]
        |6u -> (memory.[eip] <- if (status) then memory.[int r1] else memory.[int r2] ); //printf "Phi %d %d\n" r1  r2
        |_  -> failwith (sprintf "Did not understand (%d,%d, %d, %d)"  eip op r1 r2)
        
    let cmpz op memr1 = 
        match op with
        |0u -> memr1 <  0.0
        |1u -> memr1 <= 0.0
        |2u -> memr1 =  0.0
        |3u -> memr1 >= 0.0
        |4u -> memr1 >  0.0
        |_  -> failwith (sprintf "Did not understand (%d, %f)"  op memr1) 
        
    let interpretDType eip op (imm:uint32) (r1:uint32) = 
        match op with
        |0u -> (); //printf "Nop\n"
        |1u -> status <- cmpz (imm >>> 7) memory.[int r1] ; //printf "Cmpz %d %d\n" imm  r1
        |2u -> memory.[eip] <- Math.Sqrt (Math.Abs memory.[int r1]) ; //printf "Sqrt %f\n" memory.[int r1]
        |3u -> memory.[eip] <- memory.[int r1]; //printf "Copy %f\n" memory.[int r1]
        |4u -> memory.[eip] <- inport.[int r1]; //printf "Input %d" r1
        |_  -> failwith (sprintf "Did not understand (%d,%d, %d, %d)"  eip op imm r1)
        
    let rotate (x,y) = 
        //let x' = x * cos (System.Math.PI/2.0) - y * sin (System.Math.PI/2.0)
        let x' = - y 
        //let y' = x * sin (System.Math.PI/2.0) + y * cos (System.Math.PI/2.0)
        let y' = x  
        (x',y')        

    // The main loop of the interpreter.  Should be *fast*.
    let rec cycle eip = 
        if eip >= instructions.Length 
        then 0 
        else

        //printf "eip: %d :" eip
        let instruction = instructions.[eip] 
        let fourBits = instruction >>> 28
                       
        if fourBits = 0u  then 
            interpretDType eip (instruction <<< 4 >>> 28) (instruction <<< 8 >>> 22)  (instruction <<< 18 >>> 18) 
         else 
            interpretSType eip fourBits (instruction <<< 4 >>> 18) (instruction <<< 18 >>> 18) 
            
        cycle (eip+1) 
        
        
    let scaled original = float32(original * (float scale) / (2.0 * tbr))
        
    let getX x = scaled(x + tbr)
    let getY y = scaled(y + tbr)
    
    
    let thrust_control a b = 
        inport.[2] <- a 
        inport.[3] <- b
        
    let hohmann_first_delta r1 r2 =
        Math.Sqrt(mu/r1)*(Math.Sqrt((2.0 * r2) / (r1 + r2)) - 1.0)
        
    let hohmann_second_delta r1 r2 =        
        Math.Sqrt(mu/r2)*(1.0 - Math.Sqrt((2.0 *  r1) / (r1 + r2)))
        
    member this.Run() = 
    
        inport.[0x3e80] <- 1001.0 //scenario
        
        let mutable xc,yc = 0.0,0.0
        let mutable initial_radius = 0.0
        
        let earth_radius_scaled = scaled(earth_radius)
        
        let mutable lastx,lasty = 0.0,0.0
        
        let mutable first_round = true
        
        while true do 
        
            cycle 0 |> ignore
            
            let current_radius = Math.Sqrt(outport.[2]*outport.[2] + outport.[3]*outport.[3])
            let diff = Math.Abs(outport.[4] - current_radius)
            
            if debug then 
                printf "\n"
                printf "--\n"
                printf "score          : %f\n" outport.[0] 
                printf "fuel           : %f\n" outport.[1] 
                printf "earth sx       : %f\n" outport.[2] 
                printf "earth sy       : %f\n" outport.[3] 
                printf "target radius  : %f\n" outport.[4] 
                printf "current radius : %f\n" current_radius
                printf "--\n"
                printf "\n"
            
            //printf "%d %d\n" (getX outport.[2]) (getX outport.[3])
            
            printf "diff : %f\n"   diff   
            
            //run my program instructions
            
            //if first_round then
            //    initial_radius <- current_radius
            
            if done_second_thrust then
                thrust_control 0.0 0.0
                
            if done_first_thrust then 
            
                thrust_control 0.0 0.0
                
                if diff < 1.0 && not done_second_thrust then 
                    
                    let x',y' = rotate (outport.[2] , outport.[3])
                    
                    //let x' = outport.[2] - lastx
                    //let y' = outport.[3] - lasty
                    
                    //let q = Math.Sqrt(x'*x' + y'*y')
                                        
                    let deltaV = (hohmann_second_delta initial_radius outport.[4] )/ current_radius// q
                
                    let deltaVx = (x' * deltaV)  
                    let deltaVy = (y' * deltaV) 
                    
                    thrust_control deltaVx deltaVy
                    
                    done_second_thrust <- true;
                    
                    printf "second thrust (%f,%f) \n" deltaVx deltaVy
                
            if outport.[4]  <> 0.0 && not done_first_thrust (*&& not first_round*) then 
            
                let x',y' = rotate (outport.[2] , outport.[3])
                
                //let x' = outport.[2] - lastx
                //let y' = outport.[3] - lasty
                
                //let q = Math.Sqrt(x'*x' + y'*y')
                
                xc <- x'
                yc <- y'
                
                initial_radius <- current_radius
                
                let deltaV = (hohmann_first_delta current_radius outport.[4])/ (current_radius)// q
                
                let deltaVx = (x' * deltaV) 
                let deltaVy = (y' * deltaV)
                
                thrust_control deltaVx deltaVy
                
                done_first_thrust <- true;
                
                let target_radius_scaled  = float32(scaled(outport.[4]))
                let current_radius_scaled = float32(scaled(current_radius))
                
                Graphics.FromImage(image).DrawEllipse(Pens.CornflowerBlue,(float32 0),(float32 0),(float32 2.0)*target_radius_scaled,(float32 2.0)*target_radius_scaled)
                
                Graphics.FromImage(image).DrawEllipse(Pens.Yellow,target_radius_scaled-current_radius_scaled,target_radius_scaled-current_radius_scaled,(float32 2)*current_radius_scaled,(float32 2)*current_radius_scaled)
                
                Graphics.FromImage(image).DrawEllipse(Pens.Black,target_radius_scaled-earth_radius_scaled,target_radius_scaled-earth_radius_scaled,(float32 2)*earth_radius_scaled,(float32 2)*earth_radius_scaled)
                
                printf "first thrust (%f,%f) \n" deltaVx deltaVy
            
            try 
                image.SetPixel(Convert.ToInt32(getX outport.[2]),Convert.ToInt32(getY outport.[3]),Color.Red) 
            with 
                | _ -> ()
                
            Graphics.FromImage(image).FillRectangle(Brushes.White,0,0,250,100)
            Graphics.FromImage(image).DrawString( (sprintf "score: %f\n" outport.[0]),font,solidBrush,float32 0,float32 0,new StringFormat())
            Graphics.FromImage(image).DrawString( (sprintf "fuel: %f\n" outport.[1]),font,solidBrush,float32 0,float32 14,new StringFormat())
            Graphics.FromImage(image).DrawString( (sprintf "earth sx: %f\n" outport.[2]),font,solidBrush,float32 0,float32 28,new StringFormat())
            Graphics.FromImage(image).DrawString( (sprintf "earth sy: %f\n" outport.[3]),font,solidBrush,float32 0,float32 42,new StringFormat())
            Graphics.FromImage(image).DrawString( (sprintf "target radius: %f\n" outport.[4]),font,solidBrush,float32 0,float32 56,new StringFormat())
            Graphics.FromImage(image).DrawString( (sprintf "current radius: %f\n" current_radius),font,solidBrush,float32 0,float32 70,new StringFormat())
            Graphics.FromImage(image).DrawString( (sprintf "time: %d\n" time),font,solidBrush,float32 0,float32 84,new StringFormat())
            form.Invalidate(true)
            
            Application.DoEvents()    
            time <- time + uint64 1
            lastx <- outport.[2]
            lasty <- outport.[3]
            first_round <- false
            (**)


let program = File.ReadAllBytes("bin1.obf");

let image = new Bitmap(scale, scale)

Graphics.FromImage(image).FillRectangle(Brushes.White,0,0,scale,scale)

let form = new Form()
form.Text <- "Satellites"
form.Width <- scale
form.Height <- scale


let pb = new PictureBox()
pb.BorderStyle <- BorderStyle.Fixed3D
pb.Size <- new Size(scale,scale)
pb.SizeMode <- PictureBoxSizeMode.AutoSize
pb.Image <- image
pb.Show()
form.Controls.Add( pb)

form.Show()


let ee = VM(program,image,form)

ee.Run() 

