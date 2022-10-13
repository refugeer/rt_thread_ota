using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using rt_ota_packaging_tool.Properties;

namespace rt_ota_packaging_tool

	{

public class FirmwarePack : Form
{
	public struct rt_ota_rbl_hdr
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		public byte[] magic;

		public uint algo;

		public uint timestamp;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
		public byte[] name;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
		public byte[] version;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
		public byte[] sn;

		public uint crc32;

		public uint hash;

		public int size_raw;

		public int size_package;

		public uint info_crc32;
	}

	private struct Confi
	{
		public string SELEFILE;

		public string SAVEFILE;

		public string COMPRESS;

		public string CRYPT;

		public string KEY;

		public string IV;

		public string NAME;

		public string VERSION;
	}

	private MemoryStream CmprsMs = new MemoryStream();

	private const uint RT_OTA_CRYPT_ALGO_NONE = 0u;

	private const uint RT_OTA_CRYPT_ALGO_XOR = 1u;

	private const uint RT_OTA_CRYPT_ALGO_AES256 = 2u;

	private const uint RT_OTA_CMPRS_ALGO_GZIP = 256u;

	private const uint RT_OTA_CMPRS_ALGO_QUICKLZ = 512u;

	private const uint RT_OTA_CMPRS_ALGO_FASTLZ = 768u;

	private Dictionary<string, string> MsgInfo = new Dictionary<string, string>();

	private rt_ota_rbl_hdr rbl_hdr;

	private Confi cfg = default(Confi);

	private IContainer components = null;

	private Panel panTitle;

	private Label lbltitle;

	private Button btnselFirmware;

	private Button btnsavepath;

	private Label label3;

	private Label label4;

	private ComboBox cmbCompressCalculation;

	private ComboBox cmbEncryptionCalculation;

	private Label label5;

	private TextBox txtkey;

	private Label label6;

	private TextBox txtIv;

	private Label label7;

	private TextBox txtFirmwareName;

	private Label label8;

	private TextBox txtFirmwareVer;

	private Label label9;

	private Label lblPackOk;

	private Label label11;

	private Label lblhash;

	private Label label13;

	private Label lblhdr;

	private Label label15;

	private Label lblbody;

	private Label label17;

	private Label label18;

	private Label label19;

	private Label lblraw;

	private Label lblpkg;

	private Label lbltime;

	private Button btnstart;

	private Label label23;

	private Label lblVer;

	private Label lblselFirmware;

	private Label lblsavepath;
        private Button button1;
        private Label label1;

	public FirmwarePack()
	{
		InitializeComponent();
	}

	private void Form1_Load(object sender, EventArgs e)
	{
		base.AutoScaleMode = AutoScaleMode.Dpi;
		//base.Icon = Resources.RTT_64X64;
		Button button = button1;
		bool visible = (button1.Enabled = false);
		button.Visible = visible;
		cmbCompressCalculation.DataSource = "不压缩|quicklz|fastlz|gzip".Split('|');
		cmbEncryptionCalculation.DataSource = "不加密|AES256".Split('|');
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		MsgInfo.Add("ffile", "选择有效固件文件！");
		MsgInfo.Add("spath", "选择有效保存路径！");
		MsgInfo.Add("key16", "加密密钥文本长度不为16位！");
		MsgInfo.Add("key32", "加密密钥文本长度不为32位！");
		MsgInfo.Add("iv16", "IV文本长度不为16位！");
		MsgInfo.Add("fname", "请输入有效固件分区名！");
		MsgInfo.Add("fvers", "请输入有效固件版本！");
		base.MaximizeBox = false;
		lblPackOk.Text = string.Empty;
		lblPackOk.Visible = false;
		string text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
		lblVer.Text = text.Substring(0, text.LastIndexOf("."));
		lblselFirmware.Text = $"请{btnselFirmware.Text}...";
		lblsavepath.Text = $"请选择{btnsavepath.Text}...";
		btnstart.Focus();
		GetKey_IV(closed: false);
	}

	private uint GetTimeLong(DateTime createDt)
	{
		string text = createDt.ToLongDateString();
		DateTime dateTime = new DateTime(1970, 1, 1, 8, 0, 0);
		DateTime dateTime2 = createDt;
		return (uint)(dateTime2 - dateTime).TotalSeconds;
	}

	private void btnCreate_Click(object sender, EventArgs e)
	{
		try
		{
			MsgInfoNull();
			checkK_Iv_FirmwareInfo();
			byte[] bytes = Encoding.ASCII.GetBytes(txtFirmwareName.Text);
			byte[] bytes2 = Encoding.ASCII.GetBytes(txtFirmwareVer.Text);
			rbl_hdr.name = new byte[16];
			rbl_hdr.version = new byte[24];
			Array.Copy(bytes, 0, rbl_hdr.name, 0, (bytes.Length > 16) ? 16 : bytes.Length);
			Array.Copy(bytes2, 0, rbl_hdr.version, 0, (bytes2.Length > 24) ? 24 : bytes2.Length);
			rbl_hdr.magic = new byte[4] { 82, 66, 76, 0 };
			Button button = btnselFirmware;
			Button button2 = btnsavepath;
			bool flag2 = (btnstart.Enabled = false);
			bool enabled = (button2.Enabled = flag2);
			button.Enabled = enabled;
			CmprsMs = new MemoryStream();
			packStar();
			byte[] array2;
			using (FileStream fileStream = new FileStream(lblselFirmware.Text, FileMode.Open))
			{
				lblraw.Text = fileStream.Length.ToString();
				rbl_hdr.size_raw = int.Parse(lblraw.Text);
				rbl_hdr.hash = fnv1a.calc(ReadFile2Byte(fileStream), 2166136261u);
				byte[] array = new byte[fileStream.Length];
				fileStream.Read(array, 0, array.Length);
				array2 = todoCmprsMsCryptMs(fileStream);
			}
			CmprsMs.Dispose();
			Button button3 = btnselFirmware;
			Button button4 = btnsavepath;
			flag2 = (btnstart.Enabled = true);
			enabled = (button4.Enabled = flag2);
			button3.Enabled = enabled;
			lblpkg.Text = array2.Length.ToString();
			rbl_hdr.timestamp = GetTimeLong(File.GetLastWriteTime(lblselFirmware.Text));
			rbl_hdr.crc32 = fileToCRC32.GetFileCRC32(array2);
			rbl_hdr.size_package = array2.Length;
			rbl_hdr.sn = Encoding.ASCII.GetBytes("000102030405060708091011");
			byte[] sn = rbl_hdr.sn;
			byte[] sn2 = rbl_hdr.sn;
			byte b;
			rbl_hdr.sn[21] = (b = (rbl_hdr.sn[20] = 0));
			sn2[22] = (b = b);
			sn[23] = b;
			byte[] sourceArray = structSerializable.StructToBytes(rbl_hdr);
			byte[] array3 = new byte[92];
			Array.Copy(sourceArray, 0, array3, 0, 92);
			rbl_hdr.info_crc32 = fileToCRC32.GetFileCRC32(array3);
			sourceArray = structSerializable.StructToBytes(rbl_hdr);
			lblhdr.Text = rbl_hdr.info_crc32.ToString("X");
			lblbody.Text = rbl_hdr.crc32.ToString("X");
			lblhash.Text = rbl_hdr.hash.ToString("X");
			lbltime.Text = rbl_hdr.timestamp.ToString();
			using (FileStream fileStream2 = new FileStream(lblsavepath.Text, FileMode.Create))
			{
				fileStream2.Write(sourceArray, 0, sourceArray.Length);
				fileStream2.Write(array2, 0, array2.Length);
			}
			packStop();
		}
		catch (Exception ex)
		{
			packErro();
			MsgInfoNull();
			if (!MsgInfo.Values.Contains(ex.Message))
			{
				WriteLog.WriteLogInfo(ex);
			}
			MessageBox.Show("打包失败!! \r\n" + ex.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			Button button5 = btnselFirmware;
			Button button6 = btnsavepath;
			bool flag2 = (btnstart.Enabled = true);
			bool enabled = (button6.Enabled = flag2);
			button5.Enabled = enabled;
			btnstart.Focus();
		}
	}

	private void MsgInfoNull()
	{
		Label label = lblbody;
		Label label2 = lblhash;
		Label label3 = lblhdr;
		Label label4 = lblpkg;
		Label label5 = lblraw;
		string text = (lbltime.Text = string.Empty);
		string text3 = (label5.Text = text);
		string text5 = (label4.Text = text3);
		string text7 = (label3.Text = text5);
		string text10 = (label.Text = (label2.Text = text7));
	}

	private void packStar()
	{
		lblPackOk.Text = "开始打包";
		lblPackOk.ForeColor = Color.Yellow;
		lblPackOk.Visible = true;
	}

	private void packErro()
	{
		lblPackOk.Text = "打包失败";
		lblPackOk.ForeColor = Color.Red;
		lblPackOk.Visible = true;
	}

	private void packStop()
	{
		lblPackOk.Text = "打包成功";
		lblPackOk.ForeColor = Color.Green;
		lblPackOk.Visible = true;
		MessageBox.Show(this, "打包成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.None);
	}

	private void GetKey_IV(bool closed)
	{
		string text = $"{Application.ExecutablePath}.config";
			if (closed)
		{
			cfg.SELEFILE = ((lblselFirmware.Text == "请选择固件...") ? "" : lblselFirmware.Text);
			cfg.SAVEFILE = ((lblsavepath.Text == "请选择保存路径...") ? "" : lblsavepath.Text);
			cfg.COMPRESS = cmbCompressCalculation.SelectedIndex.ToString();
			cfg.CRYPT = cmbEncryptionCalculation.SelectedIndex.ToString();
			cfg.KEY = (string.IsNullOrEmpty(txtkey.Text) ? "" : txtkey.Text);
			cfg.IV = (string.IsNullOrEmpty(txtIv.Text) ? "" : txtIv.Text);
			cfg.NAME = (string.IsNullOrEmpty(txtFirmwareName.Text) ? "" : txtFirmwareName.Text);
			cfg.VERSION = (string.IsNullOrEmpty(txtFirmwareVer.Text) ? "" : txtFirmwareVer.Text);
			if (File.Exists(text))
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(text);
				XmlNode xmlNode = xmlDocument.SelectSingleNode("configuration/configs");
				if (xmlNode == null)
				{
					XmlElement xmlElement = xmlDocument.CreateElement("configs");
					xmlElement.SetAttribute("SELEFILE", cfg.SELEFILE);
					xmlElement.SetAttribute("SAVEFILE", cfg.SAVEFILE);
					xmlElement.SetAttribute("COMPRESS", cfg.COMPRESS);
					xmlElement.SetAttribute("CRYPT", cfg.CRYPT);
					xmlElement.SetAttribute("KEY", cfg.KEY);
					xmlElement.SetAttribute("IV", cfg.IV);
					xmlElement.SetAttribute("NAME", cfg.NAME);
					xmlElement.SetAttribute("VERSION", cfg.VERSION);
					XmlNode xmlNode2 = xmlDocument.SelectSingleNode("configuration");
					xmlNode2.AppendChild(xmlElement);
					xmlDocument.AppendChild(xmlNode2);
				}
				else
				{
					xmlNode.Attributes["SELEFILE"].Value = cfg.SELEFILE;
					xmlNode.Attributes["SAVEFILE"].Value = cfg.SAVEFILE;
					xmlNode.Attributes["COMPRESS"].Value = cfg.COMPRESS;
					xmlNode.Attributes["CRYPT"].Value = cfg.CRYPT;
					xmlNode.Attributes["KEY"].Value = cfg.KEY;
					xmlNode.Attributes["IV"].Value = cfg.IV;
					xmlNode.Attributes["NAME"].Value = cfg.NAME;
					xmlNode.Attributes["VERSION"].Value = cfg.VERSION;
				}
				xmlDocument.Save(text);
			}
			else
			{
				XmlDocument xmlDocument2 = new XmlDocument();
				XmlDeclaration newChild = xmlDocument2.CreateXmlDeclaration("1.0", "UTF-8", null);
				xmlDocument2.AppendChild(newChild);
				XmlElement xmlElement2 = xmlDocument2.CreateElement("configs");
				xmlElement2.SetAttribute("SELEFILE", cfg.SELEFILE);
				xmlElement2.SetAttribute("SAVEFILE", cfg.SAVEFILE);
				xmlElement2.SetAttribute("COMPRESS", cfg.COMPRESS);
				xmlElement2.SetAttribute("CRYPT", cfg.CRYPT);
				xmlElement2.SetAttribute("KEY", cfg.KEY);
				xmlElement2.SetAttribute("IV", cfg.IV);
				xmlElement2.SetAttribute("NAME", cfg.NAME);
				xmlElement2.SetAttribute("VERSION", cfg.VERSION);
				XmlNode xmlNode3 = xmlDocument2.CreateElement("configuration");
				xmlNode3.AppendChild(xmlElement2);
				xmlDocument2.AppendChild(xmlNode3);
				xmlDocument2.Save(text);
			}
		}
		else
		{
			if (!File.Exists(text))
			{
				return;
			}
			XmlDocument xmlDocument3 = new XmlDocument();
			xmlDocument3.Load(text);
			XmlNode xmlNode4 = xmlDocument3.SelectSingleNode("configuration/configs");
			if (xmlNode4 != null)
			{
				cfg.SELEFILE = ((xmlNode4.Attributes["SELEFILE"] == null) ? "" : xmlNode4.Attributes["SELEFILE"].Value);
				cfg.SAVEFILE = ((xmlNode4.Attributes["SAVEFILE"] == null) ? "" : xmlNode4.Attributes["SAVEFILE"].Value);
				cfg.COMPRESS = ((xmlNode4.Attributes["COMPRESS"] == null) ? "0" : xmlNode4.Attributes["COMPRESS"].Value);
				cfg.CRYPT = ((xmlNode4.Attributes["CRYPT"] == null) ? "0" : xmlNode4.Attributes["CRYPT"].Value);
				cfg.KEY = ((xmlNode4.Attributes["KEY"] == null) ? "" : xmlNode4.Attributes["KEY"].Value);
				cfg.IV = ((xmlNode4.Attributes["IV"] == null) ? "" : xmlNode4.Attributes["IV"].Value);
				cfg.NAME = ((xmlNode4.Attributes["NAME"] == null) ? "" : xmlNode4.Attributes["NAME"].Value);
				cfg.VERSION = ((xmlNode4.Attributes["VERSION"] == null) ? "" : xmlNode4.Attributes["VERSION"].Value);
				if (!string.IsNullOrEmpty(cfg.SELEFILE))
				{
					lblselFirmware.Text = cfg.SELEFILE;
					lblselFirmware.ForeColor = Color.Black;
				}
				if (!string.IsNullOrEmpty(cfg.SAVEFILE))
				{
					lblsavepath.Text = cfg.SAVEFILE;
					lblsavepath.ForeColor = Color.Black;
				}
				int num = int.Parse(cfg.COMPRESS);
				cmbCompressCalculation.SelectedIndex = ((num <= cmbCompressCalculation.Items.Count - 1) ? num : 0);
				num = int.Parse(cfg.CRYPT);
				cmbEncryptionCalculation.SelectedIndex = ((num <= cmbEncryptionCalculation.Items.Count - 1) ? num : 0);
				txtkey.Text = cfg.KEY;
				txtIv.Text = cfg.IV;
				txtFirmwareName.Text = cfg.NAME;
				txtFirmwareVer.Text = cfg.VERSION;
			}
		}
	}

	private byte[] todoCmprsMsCryptMs(FileStream readFile)
	{
		byte[] array = ReadFile2Byte(readFile);
		switch (cmbEncryptionCalculation.SelectedIndex)
		{
			case 0:
				switch (cmbCompressCalculation.SelectedIndex)
				{
					case 0:
						rbl_hdr.algo = 0u;
						break;
					case 1:
						rbl_hdr.algo = 512u;
						array = QuickLZCmprs(readFile);
						break;
					case 2:
						rbl_hdr.algo = 768u;
						array = Fastlz.Compress(array);
						break;
					case 3:
						{
							rbl_hdr.algo = 256u;
							using (MemoryStream memoryStream2 = new MemoryStream())
							{
								using (GZipStream gZipStream2 = new GZipStream(memoryStream2, CompressionLevel.Optimal))
								{
									gZipStream2.Write(array, 0, array.Length);
									gZipStream2.Flush();
								}
								array = memoryStream2.ToArray();
							}
							break;
						}
				}
				break;
			case 1:
				{
					byte[] readFile2;
					switch (cmbCompressCalculation.SelectedIndex)
					{
						case 0:
							rbl_hdr.algo = 2u;
							readFile2 = (byte[])array.Clone();
							break;
						case 1:
							rbl_hdr.algo = 514u;
							readFile2 = QuickLZCmprs(readFile);
							break;
						case 2:
							rbl_hdr.algo = 770u;
							readFile2 = Fastlz.Compress(ReadFile2Byte(readFile));
							break;
						case 3:
							{
								rbl_hdr.algo = 258u;
								using (MemoryStream memoryStream = new MemoryStream())
								{
									using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionLevel.Optimal))
									{
										byte[] array2 = ReadFile2Byte(readFile);
										gZipStream.Write(array2, 0, array2.Length);
										gZipStream.Flush();
									}
									readFile2 = memoryStream.ToArray();
								}
								break;
							}
						default:
							readFile2 = array;
							break;
					}
					array = Crypt(readFile2);
					break;
				}
			default:
				array = new byte[0];
				break;
		}
		return array;
	}

	private byte[] ReadFile2Byte(FileStream readFile)
	{
		byte[] array = new byte[readFile.Length];
		BinaryReader binaryReader = new BinaryReader(readFile);
		binaryReader.BaseStream.Seek(0L, SeekOrigin.Begin);
		return binaryReader.ReadBytes((int)binaryReader.BaseStream.Length);
	}

	private byte[] Crypt(byte[] readFile)
	{
		CryptApi cryptApi = new CryptApi(CryptApi.CryptType.AES_256_CBC, txtkey.Text, txtIv.Text);
		byte[] inputdata = FileStreamToByte(readFile, padding: true);
		return cryptApi.Encrypt(inputdata);
	}

	private byte[] QuickLZCmprs(FileStream readFile)
	{
		long num = new QuickLZ_DC().Compress(readFile, CmprsMs, 0L);
		return CmprsMs.ToArray();
	}

	private byte[] FileStreamToByte(byte[] readFile, bool padding)
	{
		byte[] array2;
		if (padding)
		{
			int num = 16 - readFile.Length % 16;
			byte[] array = new byte[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = (byte)num;
			}
			array2 = new byte[readFile.Length + num];
			Array.Copy(readFile, array2, readFile.Length);
			Array.Copy(array, 0, array2, readFile.Length, array.Length);
		}
		else
		{
			array2 = readFile;
		}
		return array2;
	}

	private void btnselFirmware_Click(object sender, EventArgs e)
	{
		OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.Multiselect = false;
		openFileDialog.Title = btnselFirmware.Text;
		openFileDialog.Filter = "BIN|*.bin";
		if (openFileDialog.ShowDialog() == DialogResult.OK)
		{
			lblselFirmware.Text = openFileDialog.FileName;
			string directoryName = Path.GetDirectoryName(lblselFirmware.Text);
			string fileName = Path.GetFileName(lblselFirmware.Text);
			string path = fileName.Replace(Path.GetExtension(fileName), ".rbl");
			lblsavepath.Text = Path.Combine(directoryName, path);
			lblselFirmware.ForeColor = Color.Black;
			lblsavepath.ForeColor = Color.Black;
		}
	}

	private void btnsavepath_Click(object sender, EventArgs e)
	{
		if (!File.Exists(lblselFirmware.Text))
		{
			MessageBox.Show(this, "请选择有效固件文件！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			return;
		}
		SaveFileDialog saveFileDialog = new SaveFileDialog
		{
			DefaultExt = ".rbl",
			Title = "保存文件",
			Filter = "RBL|*.rbl",
			FileName = Path.GetFileName(lblsavepath.Text)
		};
		if (File.Exists(lblsavepath.Text))
		{
			saveFileDialog.InitialDirectory = Path.GetDirectoryName(lblsavepath.Text);
		}
		else
		{
			saveFileDialog.InitialDirectory = Path.GetDirectoryName(lblselFirmware.Text);
		}
		if (saveFileDialog.ShowDialog() == DialogResult.OK)
		{
			lblsavepath.ForeColor = Color.Black;
			lblsavepath.Text = saveFileDialog.FileName;
		}
	}

	private void txtkey_Leave(object sender, EventArgs e)
	{
		if (txtkey.TextLength == 16 || txtkey.TextLength == 32)
		{
			txtkey.BackColor = Color.Empty;
		}
	}

	private void txtIv_Leave(object sender, EventArgs e)
	{
		if (txtIv.TextLength == 16)
		{
			txtIv.BackColor = Color.Empty;
		}
	}

	private void txtFirmwareName_Leave(object sender, EventArgs e)
	{
		if (txtFirmwareName.TextLength >= 0)
		{
			txtFirmwareName.BackColor = Color.Empty;
		}
	}

	private void txtFirmwareVer_Leave(object sender, EventArgs e)
	{
		if (txtFirmwareVer.TextLength >= 0)
		{
			txtFirmwareVer.BackColor = Color.Empty;
		}
	}

	private void checkK_Iv_FirmwareInfo()
	{
		if (!File.Exists(lblselFirmware.Text))
		{
			throw new Exception(MsgInfo["ffile"]);
		}
		string text = lblsavepath.Text;
		if (text.Contains("\\"))
		{
			if (!Directory.Exists(text.Substring(0, text.LastIndexOf("\\"))))
			{
				throw new Exception(MsgInfo["spath"]);
			}
			if (cmbEncryptionCalculation.SelectedIndex > 0)
			{
				if (cmbEncryptionCalculation.SelectedValue.ToString().Contains("128") && txtkey.TextLength != 16)
				{
					txtkey.BackColor = Color.Red;
					txtkey.Focus();
					throw new Exception(MsgInfo["key16"]);
				}
				if (cmbEncryptionCalculation.SelectedValue.ToString().Contains("256") && txtkey.TextLength != 32)
				{
					txtkey.BackColor = Color.Red;
					txtkey.Focus();
					throw new Exception(MsgInfo["key32"]);
				}
			}
			else
			{
				txtkey.BackColor = Color.Empty;
			}
			if (cmbEncryptionCalculation.SelectedIndex > 0 && txtIv.TextLength != 16)
			{
				txtIv.BackColor = Color.Red;
				txtIv.Focus();
				throw new Exception(MsgInfo["iv16"]);
			}
			txtIv.BackColor = Color.Empty;
			if (txtFirmwareName.TextLength == 0)
			{
				txtFirmwareName.BackColor = Color.Red;
				txtFirmwareName.Focus();
				throw new Exception(MsgInfo["fname"]);
			}
			txtFirmwareName.BackColor = Color.Empty;
			if (txtFirmwareVer.TextLength == 0)
			{
				txtFirmwareVer.BackColor = Color.Red;
				txtFirmwareVer.Focus();
				throw new Exception(MsgInfo["fvers"]);
			}
			txtFirmwareVer.BackColor = Color.Empty;
			return;
		}
		throw new Exception(MsgInfo["spath"]);
	}

	private void cmbEncryptionCalculation_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (cmbEncryptionCalculation.SelectedValue.ToString().Contains("128"))
		{
			txtkey.MaxLength = 16;
		}
		else if (cmbEncryptionCalculation.SelectedValue.ToString().Contains("256"))
		{
			txtkey.MaxLength = 32;
		}
	}

	private void button1_Click(object sender, EventArgs e)
	{
		SaveFileDialog saveFileDialog = new SaveFileDialog();
		OpenFileDialog openFileDialog = new OpenFileDialog();
		if (openFileDialog.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		byte[] array = new byte[openFileDialog.OpenFile().Length];
		int num = Marshal.SizeOf(default(rt_ota_rbl_hdr));
		byte[] array2 = new byte[array.Length - num];
		BinaryReader binaryReader = new BinaryReader(openFileDialog.OpenFile());
		binaryReader.BaseStream.Seek(0L, SeekOrigin.Begin);
		array = binaryReader.ReadBytes((int)binaryReader.BaseStream.Length - num);
		array2 = array.Skip(num).Take(array2.Length).ToArray();
		rt_ota_rbl_hdr rt_ota_rbl_hdr = (rt_ota_rbl_hdr)structSerializable.BytesToStruct(array.Skip(0).Take(num).ToArray(), typeof(rt_ota_rbl_hdr));
		if (DialogResult.OK == saveFileDialog.ShowDialog())
		{
			byte[] array3 = new CryptApi(CryptApi.CryptType.AES_256_CBC, "11111111111111111111111111111111", "2222222222222222").Decrypt(array2, rt_ota_rbl_hdr.size_raw);
			MemoryStream memoryStream = new MemoryStream();
			int num2 = 0;
			StringBuilder stringBuilder = new StringBuilder();
			int num3 = 1;
			for (int i = 0; i < array3.Length; i += num2)
			{
				byte[] array4 = array3.Skip(i).Take(4).ToArray();
				int num4 = 0;
				num4 += array4[0] * 16777216;
				num4 += array4[1] * 65536 + num4 / 16777216;
				num4 += array4[2] * 256 + array4[1] * 65536 + num4 / 16777216;
				num4 += array4[3];
				byte[] array5 = array3.Skip(i).Take(num4).ToArray();
				byte[] array6 = new byte[4366];
				byte[] array7 = Fastlz.Decompress(array5, array6.Length);
				num2 = array5.Length + 4;
				Console.WriteLine($"第{num3}次");
				stringBuilder.AppendLine($"第{num3++}次解压前数据长度==>{num4}");
				memoryStream.Write(array7, 0, array7.Length);
			}
			FileStream fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create);
			fileStream.Write(memoryStream.ToArray(), 0, memoryStream.ToArray().Length);
			fileStream.Flush();
			fileStream.Close();
		}
	}

	private void FirmwarePack_FormClosing(object sender, FormClosingEventArgs e)
	{
		GetKey_IV(closed: true);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
            this.panTitle = new System.Windows.Forms.Panel();
            this.lbltitle = new System.Windows.Forms.Label();
            this.btnselFirmware = new System.Windows.Forms.Button();
            this.btnsavepath = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.cmbCompressCalculation = new System.Windows.Forms.ComboBox();
            this.cmbEncryptionCalculation = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtkey = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtIv = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtFirmwareName = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtFirmwareVer = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.lblPackOk = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.lblhash = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.lblhdr = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.lblbody = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.lblraw = new System.Windows.Forms.Label();
            this.lblpkg = new System.Windows.Forms.Label();
            this.lbltime = new System.Windows.Forms.Label();
            this.btnstart = new System.Windows.Forms.Button();
            this.label23 = new System.Windows.Forms.Label();
            this.lblVer = new System.Windows.Forms.Label();
            this.lblselFirmware = new System.Windows.Forms.Label();
            this.lblsavepath = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.panTitle.SuspendLayout();
            this.SuspendLayout();
            // 
            // panTitle
            // 
            this.panTitle.BackColor = System.Drawing.Color.White;
            this.panTitle.Controls.Add(this.lbltitle);
            this.panTitle.Location = new System.Drawing.Point(-1, -1);
            this.panTitle.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panTitle.Name = "panTitle";
            this.panTitle.Size = new System.Drawing.Size(587, 46);
            this.panTitle.TabIndex = 0;
            // 
            // lbltitle
            // 
            this.lbltitle.Font = new System.Drawing.Font("微软雅黑", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbltitle.Location = new System.Drawing.Point(150, 7);
            this.lbltitle.Name = "lbltitle";
            this.lbltitle.Size = new System.Drawing.Size(341, 33);
            this.lbltitle.TabIndex = 0;
            this.lbltitle.Text = "OTA 固件打包器";
            this.lbltitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnselFirmware
            // 
            this.btnselFirmware.BackColor = System.Drawing.Color.White;
            this.btnselFirmware.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnselFirmware.Location = new System.Drawing.Point(35, 56);
            this.btnselFirmware.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnselFirmware.Name = "btnselFirmware";
            this.btnselFirmware.Size = new System.Drawing.Size(99, 30);
            this.btnselFirmware.TabIndex = 0;
            this.btnselFirmware.Text = "选择固件";
            this.btnselFirmware.UseVisualStyleBackColor = false;
            this.btnselFirmware.Click += new System.EventHandler(this.btnselFirmware_Click);
            // 
            // btnsavepath
            // 
            this.btnsavepath.BackColor = System.Drawing.Color.White;
            this.btnsavepath.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnsavepath.Location = new System.Drawing.Point(35, 101);
            this.btnsavepath.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnsavepath.Name = "btnsavepath";
            this.btnsavepath.Size = new System.Drawing.Size(99, 30);
            this.btnsavepath.TabIndex = 1;
            this.btnsavepath.Text = "保存路径";
            this.btnsavepath.UseVisualStyleBackColor = false;
            this.btnsavepath.Click += new System.EventHandler(this.btnsavepath_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(40, 149);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(74, 21);
            this.label3.TabIndex = 5;
            this.label3.Text = "压缩算法";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.Location = new System.Drawing.Point(40, 189);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(74, 21);
            this.label4.TabIndex = 6;
            this.label4.Text = "加密算法";
            // 
            // cmbCompressCalculation
            // 
            this.cmbCompressCalculation.BackColor = System.Drawing.Color.White;
            this.cmbCompressCalculation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCompressCalculation.FormattingEnabled = true;
            this.cmbCompressCalculation.Location = new System.Drawing.Point(151, 150);
            this.cmbCompressCalculation.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cmbCompressCalculation.Name = "cmbCompressCalculation";
            this.cmbCompressCalculation.Size = new System.Drawing.Size(404, 27);
            this.cmbCompressCalculation.TabIndex = 2;
            // 
            // cmbEncryptionCalculation
            // 
            this.cmbEncryptionCalculation.BackColor = System.Drawing.Color.White;
            this.cmbEncryptionCalculation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbEncryptionCalculation.FormattingEnabled = true;
            this.cmbEncryptionCalculation.Location = new System.Drawing.Point(151, 190);
            this.cmbEncryptionCalculation.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cmbEncryptionCalculation.Name = "cmbEncryptionCalculation";
            this.cmbEncryptionCalculation.Size = new System.Drawing.Size(404, 27);
            this.cmbEncryptionCalculation.TabIndex = 3;
            this.cmbEncryptionCalculation.SelectedIndexChanged += new System.EventHandler(this.cmbEncryptionCalculation_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label5.Location = new System.Drawing.Point(40, 228);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(74, 21);
            this.label5.TabIndex = 9;
            this.label5.Text = "加密密钥";
            // 
            // txtkey
            // 
            this.txtkey.Location = new System.Drawing.Point(151, 229);
            this.txtkey.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtkey.MaxLength = 16;
            this.txtkey.Name = "txtkey";
            this.txtkey.Size = new System.Drawing.Size(404, 25);
            this.txtkey.TabIndex = 4;
            this.txtkey.Leave += new System.EventHandler(this.txtkey_Leave);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label6.Location = new System.Drawing.Point(40, 262);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(68, 21);
            this.label6.TabIndex = 9;
            this.label6.Text = "加密  IV";
            // 
            // txtIv
            // 
            this.txtIv.Location = new System.Drawing.Point(151, 263);
            this.txtIv.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtIv.MaxLength = 16;
            this.txtIv.Name = "txtIv";
            this.txtIv.Size = new System.Drawing.Size(404, 25);
            this.txtIv.TabIndex = 5;
            this.txtIv.Leave += new System.EventHandler(this.txtIv_Leave);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label7.Location = new System.Drawing.Point(40, 302);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(90, 21);
            this.label7.TabIndex = 9;
            this.label7.Text = "固件分区名";
            // 
            // txtFirmwareName
            // 
            this.txtFirmwareName.Location = new System.Drawing.Point(151, 302);
            this.txtFirmwareName.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtFirmwareName.Name = "txtFirmwareName";
            this.txtFirmwareName.Size = new System.Drawing.Size(174, 25);
            this.txtFirmwareName.TabIndex = 6;
            this.txtFirmwareName.Leave += new System.EventHandler(this.txtFirmwareName_Leave);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label8.Location = new System.Drawing.Point(336, 302);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(74, 21);
            this.label8.TabIndex = 9;
            this.label8.Text = "固件版本";
            // 
            // txtFirmwareVer
            // 
            this.txtFirmwareVer.Location = new System.Drawing.Point(416, 302);
            this.txtFirmwareVer.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtFirmwareVer.Name = "txtFirmwareVer";
            this.txtFirmwareVer.Size = new System.Drawing.Size(139, 25);
            this.txtFirmwareVer.TabIndex = 7;
            this.txtFirmwareVer.Leave += new System.EventHandler(this.txtFirmwareVer_Leave);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label9.Location = new System.Drawing.Point(38, 356);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(63, 21);
            this.label9.TabIndex = 9;
            this.label9.Text = "结果 ：";
            // 
            // lblPackOk
            // 
            this.lblPackOk.Font = new System.Drawing.Font("微软雅黑", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblPackOk.ForeColor = System.Drawing.Color.Green;
            this.lblPackOk.Location = new System.Drawing.Point(149, 352);
            this.lblPackOk.Name = "lblPackOk";
            this.lblPackOk.Size = new System.Drawing.Size(97, 32);
            this.lblPackOk.TabIndex = 9;
            this.lblPackOk.Text = "打包成功";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.ForeColor = System.Drawing.Color.Gray;
            this.label11.Location = new System.Drawing.Point(41, 419);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(109, 20);
            this.label11.TabIndex = 9;
            this.label11.Text = "HASH_CODE   :";
            // 
            // lblhash
            // 
            this.lblhash.AutoSize = true;
            this.lblhash.ForeColor = System.Drawing.Color.Gray;
            this.lblhash.Location = new System.Drawing.Point(142, 419);
            this.lblhash.Name = "lblhash";
            this.lblhash.Size = new System.Drawing.Size(93, 20);
            this.lblhash.TabIndex = 9;
            this.lblhash.Text = "                     ";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.ForeColor = System.Drawing.Color.Gray;
            this.label13.Location = new System.Drawing.Point(41, 450);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(104, 20);
            this.label13.TabIndex = 9;
            this.label13.Text = "HDR_CRC32   :";
            // 
            // lblhdr
            // 
            this.lblhdr.AutoSize = true;
            this.lblhdr.ForeColor = System.Drawing.Color.Gray;
            this.lblhdr.Location = new System.Drawing.Point(142, 450);
            this.lblhdr.Name = "lblhdr";
            this.lblhdr.Size = new System.Drawing.Size(93, 20);
            this.lblhdr.TabIndex = 9;
            this.lblhdr.Text = "                     ";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.ForeColor = System.Drawing.Color.Gray;
            this.label15.Location = new System.Drawing.Point(41, 480);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(104, 20);
            this.label15.TabIndex = 9;
            this.label15.Text = "BODY_CRC32 :";
            // 
            // lblbody
            // 
            this.lblbody.AutoSize = true;
            this.lblbody.ForeColor = System.Drawing.Color.Gray;
            this.lblbody.Location = new System.Drawing.Point(142, 480);
            this.lblbody.Name = "lblbody";
            this.lblbody.Size = new System.Drawing.Size(93, 20);
            this.lblbody.TabIndex = 9;
            this.lblbody.Text = "                     ";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.ForeColor = System.Drawing.Color.Gray;
            this.label17.Location = new System.Drawing.Point(236, 419);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(96, 20);
            this.label17.TabIndex = 9;
            this.label17.Text = "RAW_SIZE    :";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.ForeColor = System.Drawing.Color.Gray;
            this.label18.Location = new System.Drawing.Point(236, 450);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(95, 20);
            this.label18.TabIndex = 9;
            this.label18.Text = "PKG_SIZE     :";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.ForeColor = System.Drawing.Color.Gray;
            this.label19.Location = new System.Drawing.Point(236, 480);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(99, 20);
            this.label19.TabIndex = 9;
            this.label19.Text = "TIMESTAMP :";
            // 
            // lblraw
            // 
            this.lblraw.AutoSize = true;
            this.lblraw.ForeColor = System.Drawing.Color.Gray;
            this.lblraw.Location = new System.Drawing.Point(337, 419);
            this.lblraw.Name = "lblraw";
            this.lblraw.Size = new System.Drawing.Size(93, 20);
            this.lblraw.TabIndex = 9;
            this.lblraw.Text = "                     ";
            // 
            // lblpkg
            // 
            this.lblpkg.AutoSize = true;
            this.lblpkg.ForeColor = System.Drawing.Color.Gray;
            this.lblpkg.Location = new System.Drawing.Point(337, 450);
            this.lblpkg.Name = "lblpkg";
            this.lblpkg.Size = new System.Drawing.Size(93, 20);
            this.lblpkg.TabIndex = 9;
            this.lblpkg.Text = "                     ";
            // 
            // lbltime
            // 
            this.lbltime.AutoSize = true;
            this.lbltime.ForeColor = System.Drawing.Color.Gray;
            this.lbltime.Location = new System.Drawing.Point(337, 480);
            this.lbltime.Name = "lbltime";
            this.lbltime.Size = new System.Drawing.Size(93, 20);
            this.lbltime.TabIndex = 9;
            this.lbltime.Text = "                     ";
            // 
            // btnstart
            // 
            this.btnstart.BackColor = System.Drawing.Color.White;
            this.btnstart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnstart.Font = new System.Drawing.Font("微软雅黑", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnstart.Location = new System.Drawing.Point(442, 382);
            this.btnstart.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnstart.Name = "btnstart";
            this.btnstart.Size = new System.Drawing.Size(113, 115);
            this.btnstart.TabIndex = 8;
            this.btnstart.Text = "开始打包";
            this.btnstart.UseVisualStyleBackColor = false;
            this.btnstart.Click += new System.EventHandler(this.btnCreate_Click);
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.ForeColor = System.Drawing.Color.DarkGray;
            this.label23.Location = new System.Drawing.Point(5, 535);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(0, 20);
            this.label23.TabIndex = 9;
            // 
            // lblVer
            // 
            this.lblVer.AutoSize = true;
            this.lblVer.ForeColor = System.Drawing.Color.DarkGray;
            this.lblVer.Location = new System.Drawing.Point(521, 535);
            this.lblVer.Name = "lblVer";
            this.lblVer.Size = new System.Drawing.Size(39, 20);
            this.lblVer.TabIndex = 9;
            this.lblVer.Text = "1.0.0";
            // 
            // lblselFirmware
            // 
            this.lblselFirmware.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblselFirmware.ForeColor = System.Drawing.Color.DarkGray;
            this.lblselFirmware.Location = new System.Drawing.Point(151, 57);
            this.lblselFirmware.Name = "lblselFirmware";
            this.lblselFirmware.Size = new System.Drawing.Size(404, 30);
            this.lblselFirmware.TabIndex = 5;
            this.lblselFirmware.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblsavepath
            // 
            this.lblsavepath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblsavepath.ForeColor = System.Drawing.Color.DarkGray;
            this.lblsavepath.Location = new System.Drawing.Point(151, 102);
            this.lblsavepath.Name = "lblsavepath";
            this.lblsavepath.Size = new System.Drawing.Size(404, 30);
            this.lblsavepath.TabIndex = 5;
            this.lblsavepath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.DarkGray;
            this.label1.Location = new System.Drawing.Point(488, 535);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 20);
            this.label1.TabIndex = 9;
            this.label1.Text = " Ver:";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(302, 356);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 10;
            this.button1.Text = "解压";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // FirmwarePack
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(584, 561);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnstart);
            this.Controls.Add(this.txtFirmwareVer);
            this.Controls.Add(this.txtFirmwareName);
            this.Controls.Add(this.txtIv);
            this.Controls.Add(this.txtkey);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.lbltime);
            this.Controls.Add(this.lblbody);
            this.Controls.Add(this.lblpkg);
            this.Controls.Add(this.lblhdr);
            this.Controls.Add(this.lblraw);
            this.Controls.Add(this.lblhash);
            this.Controls.Add(this.lblPackOk);
            this.Controls.Add(this.label19);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblVer);
            this.Controls.Add(this.label23);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.label18);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.label17);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.cmbEncryptionCalculation);
            this.Controls.Add(this.cmbCompressCalculation);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lblselFirmware);
            this.Controls.Add(this.lblsavepath);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnsavepath);
            this.Controls.Add(this.btnselFirmware);
            this.Controls.Add(this.panTitle);
            this.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "FirmwarePack";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "                     ";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FirmwarePack_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panTitle.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

	}
}
}